if [ -z "$1" ]
  then
    echo "Usage: deploy.sh user@host"
    exit
fi
scp ./bin/Debug/net461/*.* "$1":~/legocar/dotnet