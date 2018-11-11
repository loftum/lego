if [ -z "$1" -o -z "$2" -o -z "$3" ]
  then
    echo "Usage: deploy.sh project framework user@host"
    echo "Ex: deploy.sh LegoCarServer netcorapp2.1 user@host"
    echo
    exit
fi
dotnet publish ./"$1"/ -f "$2"
scp -r ./"$1"/bin/Debug/$2/publish/* "$3":~/"$1"