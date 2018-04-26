if [ -z "$1" -o -z "$2" ]
  then
    echo "Usage: deploy.sh project user@host"
    exit
fi
scp ./"$1"/bin/Debug/net461/*.* "$2":~/"$1"