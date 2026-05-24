import os
import shutil
import subprocess
import sys
import zipfile

# Step 1: TypeScript + Vite build (produces React bundle in dist/)
print("Building React bundle...")
result = subprocess.run(["npm", "run", "build"], shell=True)
if result.returncode != 0:
    print("npm run build failed")
    sys.exit(1)

# Step 2: Keycloakify build (produces the real, Maven-compiled jar in dist_keycloak/)
print("Running keycloakify build...")
result = subprocess.run(["npx", "keycloakify", "build"], shell=True)
if result.returncode != 0:
    print("keycloakify build failed")
    sys.exit(1)

# Step 3: Locate the Keycloakify-produced jar.
# Keycloakify v11 emits one or more jars in dist_keycloak/. The exact filename
# depends on keycloakVersionTargets in package.json. Pick whatever is there.
dist_keycloak = "dist_keycloak"
if not os.path.isdir(dist_keycloak):
    print(f"Expected directory '{dist_keycloak}' not found. Did 'keycloakify build' succeed?")
    sys.exit(1)

jars = [f for f in os.listdir(dist_keycloak) if f.endswith(".jar")]
if not jars:
    print(f"No .jar files found in {dist_keycloak}/. 'keycloakify build' produced no output.")
    sys.exit(1)

# Prefer the "all other versions" / generic jar if multiple are present;
# otherwise just take the first. Adjust this if you target specific KC versions.
preferred = None
for name in jars:
    if "all-other-versions" in name or "screendrafts.jar" in name:
        preferred = name
        break
if preferred is None:
    preferred = jars[0]

source_jar = os.path.join(dist_keycloak, preferred)
print(f"Selected jar: {source_jar}")

# Step 4: Sanity check — verify the jar contains the FTL templates and
# theme registration that Keycloak needs. If these are missing, the jar is
# incomplete and login will fail with kcContext undefined.
required_entries = [
    "META-INF/keycloak-themes.json",
]
required_prefixes = [
    "theme/screendrafts/login/",
]

with zipfile.ZipFile(source_jar, "r") as zf:
    names = zf.namelist()

    missing = [e for e in required_entries if e not in names]
    if missing:
        print("Jar is missing required entries:")
        for m in missing:
            print(f"  - {m}")
        sys.exit(1)

    for prefix in required_prefixes:
        if not any(n.startswith(prefix) for n in names):
            print(f"Jar contains no entries under '{prefix}'")
            sys.exit(1)

    ftl_count = sum(1 for n in names if n.endswith(".ftl"))
    if ftl_count == 0:
        print("Jar contains no .ftl templates. Keycloak will not be able to render pages.")
        sys.exit(1)

    print(f"Jar contents OK: {ftl_count} .ftl templates, {len(names)} total entries")

# Step 5: Copy to api/res/
dest_dir = os.path.join("..", "screendrafts.api", "res")
dest = os.path.join(dest_dir, "keycloak-theme-screendrafts.jar")
if not os.path.isdir(dest_dir):
    print(f"Destination directory not found: {dest_dir}")
    print("Copy the jar manually from:")
    print(f"  {os.path.abspath(source_jar)}")
    sys.exit(1)

shutil.copy(source_jar, dest)
size_kb = os.path.getsize(dest) / 1024
print(f"Copied to {dest} ({size_kb:.0f} KB)")
print("Done. Restart the screendrafts.identity container to pick up the new jar.")