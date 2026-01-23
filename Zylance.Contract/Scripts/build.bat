protoc ^
    --plugin=./node_modules/.bin/protoc-gen-ts_proto.cmd ^
    --ts_proto_opt=esModuleInterop ^
    --ts_proto_opt=env=browser ^
    --ts_proto_opt=forceLong=string ^
    --ts_proto_opt=outputEncodeMethods=false ^
    --ts_proto_opt=outputJsonMethods=true ^
    --ts_proto_opt=outputClientImpl=false ^
    --ts_proto_opt=nestJs=false ^
    --ts_proto_out=../Zylance.UI/Src/Generated ^
    ./Src/Zylance.proto