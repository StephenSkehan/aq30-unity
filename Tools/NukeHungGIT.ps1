# Nuke the push and any helper processes
taskkill /F /T /IM git.exe
taskkill /F /T /IM git-remote-https.exe
taskkill /F /T /IM ssh.exe
