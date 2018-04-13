if [ -z "$1" ]
  then
    echo "Usage: deploy.sh user@host"
    exit
fi
scp *.py "$1":~/legocar/python