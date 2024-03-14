#! /bin/bash
# To get your private access token, go to your profile page in gitlab and go to "Access Tokens"
# Then you can either set it temporarily with export GITLAB_PRIVATE_ACCESS_TOKEN=<token> or add this command to your .bashrc
# Access tokens should not be committed to the repository, so do not replace below variable with your access token

# IMPORTANT: sudo does not use your environment variables per default, so you need to use sudo -E \.deploy.sh

mkdir -p /var/local/prime/optimization
cd /var/local/prime/optimization
# Backup Server Config File
mkdir -p ConfigTmp
cp -vf Optimization.StartUp/bin/Release/*.config ConfigTmp/
echo $PWD
echo "PRIVATE ACCESS TOKEN: ${GITLAB_PRIVATE_ACCESS_TOKEN}     (for sudo remember to use -E flag)"
curl --header "PRIVATE-TOKEN: ${GITLAB_PRIVATE_ACCESS_TOKEN}" --output artifacts.zip "https://gitlab.cc-asp.fraunhofer.de/api/v4/projects/6476/jobs/artifacts/development/download?job=build:halcon:mono"
7z x -aoa artifacts.zip
cp -vf ConfigTmp/*.config Optimization.StartUp/bin/Release/
rm artifacts.zip
rm -r ConfigTmp
mono Optimization.StartUp/bin/Release/Optimization.StartUp.exe
