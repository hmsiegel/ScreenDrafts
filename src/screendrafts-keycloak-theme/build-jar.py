import zipfile
import os
import subprocess
import sys

# Step 1: build
print("Building...")
result = subprocess.run(["npm", "run", "build"], shell=True)
if result.returncode != 0:
    print("Build failed")
    sys.exit(1)

# Step 2: keycloakify build
print("Running keycloakify build...")
result = subprocess.run(["npx", "keycloakify", "build"], shell=True)
if result.returncode != 0:
    print("Keycloakify build failed")
    sys.exit(1)

# Step 3: package JAR
print("Packaging JAR...")
resources = "node_modules/.cache/keycloakify/maven/keycloak-theme-screendrafts/src/main/resources"
output = "keycloak-theme-screendrafts.jar"

manifest = (
    "Manifest-Version: 1.0\r\n"
    "Archiver-Version: Plexus Archiver\r\n"
    "Created-By: Apache Maven 3.8.7\r\n"
    "\r\n"
)

with zipfile.ZipFile(output, 'w', zipfile.ZIP_DEFLATED) as zf:
    zf.writestr('META-INF/MANIFEST.MF', manifest)
    for root, _, files in os.walk(resources):
        for f in files:
            abs_path = os.path.join(root, f)
            zf.write(abs_path, os.path.relpath(abs_path, resources))

# Step 4: verify
result = zipfile.ZipFile(output).testzip()
if result is not None:
    print(f"JAR corrupt at entry: {result}")
    sys.exit(1)

size = os.path.getsize(output) / 1024
print(f"JAR OK: {output} ({size:.0f} KB)")

# Step 5: copy to res/
dest = os.path.join("..", "screendrafts.api", "res", "keycloak-theme-screendrafts.jar")
if os.path.exists(os.path.dirname(dest)):
    import shutil
    shutil.copy(output, dest)
    print(f"Copied to {dest}")
else:
    print(f"Could not find {dest} — copy manually")