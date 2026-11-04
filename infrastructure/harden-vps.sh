#!/bin/bash
# infrastructure/test/harden-vps.sh
#
# Hardens a fresh VPS the way PROD is hardened. Run as root, in two stages,
# AFTER vps-setup.sh and AFTER `tailscale up` (stage 2 needs the tailnet).
#
#   ./harden-vps.sh users   # create the admin and CI accounts, install their keys
#   ./harden-vps.sh lock    # turn off root and password SSH, move SSH to Tailscale
#
# Keep your root PuTTY session open until the end. Between the stages, log in
# as the admin account from a NEW window to prove it works.

set -euo pipefail
[ "$(id -u)" -eq 0 ] || { echo "Run as root."; exit 1; }

ADMIN_USER="hmsiegel"
CI_USER="deploy"
BASE="/opt/screendrafts"

stage="${1:-}"

install_key() {   # install_key USER "ssh-ed25519 AAAA… comment"
  local user="$1" key="$2" dir="/home/$1/.ssh"
  install -d -m 700 -o "${user}" -g "${user}" "${dir}"
  printf '%s\n' "${key}" > "${dir}/authorized_keys"
  chown "${user}:${user}" "${dir}/authorized_keys"
  chmod 600 "${dir}/authorized_keys"
}

case "${stage}" in
users)
  echo "==> Admin account: ${ADMIN_USER} (sudo + docker)"
  id "${ADMIN_USER}" >/dev/null 2>&1 || useradd -m -s /bin/bash "${ADMIN_USER}"
  usermod -aG sudo,docker "${ADMIN_USER}"
  echo "Set a password for ${ADMIN_USER}. It is used only for sudo, never for SSH."
  passwd "${ADMIN_USER}"
  read -rp "Paste ${ADMIN_USER}'s PUBLIC key (one line): " ADMIN_KEY
  install_key "${ADMIN_USER}" "${ADMIN_KEY}"

  echo "==> CI account: ${CI_USER} (docker group, no sudo, no password)"
  id "${CI_USER}" >/dev/null 2>&1 || useradd -m -s /bin/bash "${CI_USER}"
  usermod -aG docker "${CI_USER}"
  passwd -l "${CI_USER}" >/dev/null
  read -rp "Paste the CI deploy PUBLIC key (one line): " CI_KEY
  install_key "${CI_USER}" "${CI_KEY}"

  echo "==> Give the CI account the deploy directory"
  # deploy.yml copies the compose file and Caddyfile here and writes IMAGE_TAG into .env.
  chown -R "${CI_USER}:${CI_USER}" "${BASE}"
  [ -f "${BASE}/.env" ] && chmod 600 "${BASE}/.env"

  echo
  echo "Stage 1 done. Now, from a NEW window on your PC (the server may not be on"
  echo "Tailscale yet, so use its public IP for this first test):"
  echo "    ssh ${ADMIN_USER}@<server-ip>        then:  sudo -v ; docker ps"
  echo "    ssh -i <ci-private-key> ${CI_USER}@<server-ip>   then:  docker ps"
  echo "Then run:  tailscale up --hostname=<name> --advertise-tags=<tag>   and run stage 2."
  ;;

lock)
  tailscale ip -4 >/dev/null 2>&1 || { echo "Tailscale is not up. Run 'tailscale up' first."; exit 1; }
  echo "Tailscale address: $(tailscale ip -4)"
  read -rp "Did you log in as ${ADMIN_USER} and as ${CI_USER} successfully? (yes/no) " ok
  [ "${ok}" = "yes" ] || { echo "Test both logins first."; exit 1; }

  echo "==> SSH: no root, no passwords"
  # The 00- prefix matters: sshd uses the first value it reads, and Ubuntu's
  # 50-cloud-init.conf can re-enable passwords if it is read first.
  cat > /etc/ssh/sshd_config.d/00-hardening.conf <<'CONF'
PermitRootLogin no
PasswordAuthentication no
CONF
  sshd -t
  systemctl reload ssh
  sshd -T | grep -Ei "permitrootlogin|passwordauthentication"

  echo "==> ufw: allow SSH on the Tailscale interface"
  ufw allow in on tailscale0 to any port 22 proto tcp
  ufw status numbered
  echo
  echo "Now, from a NEW window, SSH in over the Tailscale address as ${ADMIN_USER}."
  echo "When that works, delete the PUBLIC port 22 rules (not the tailscale0 ones),"
  echo "highest number first, IPv4 and (v6) both:   ufw delete <n>"
  echo "Keep 80/tcp, 443/tcp and 443/udp. Then remove port 22 from the Hetzner Cloud Firewall."
  echo "Check from your PC:  Test-NetConnection <public-ip> -Port 22   ->  TcpTestSucceeded : False"
  ;;

*)
  echo "usage: $0 users|lock"
  exit 1
  ;;
esac
