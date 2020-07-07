if [ -z "$1" -o -z "$2" ]
  then
    echo "Usage: deploy.sh project user@host"
    echo "Ex: deploy.sh LegoCarServer user@host"
    echo
    exit
fi
scp ./"$1"/bin/Debug/net472/*.* "$2":~/"$1"