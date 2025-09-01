#!/bin/bash

# === 입력 파라미터 ===
PROJECT_PATH="$1"      # GameServer/../../ 와 같은 프로젝트 기준 경로
OUTPUT_REL_PATH="$2"   # 출력 상대 경로(Server/..., Client/...)
PROGRAM_TYPE="$3"      # 1: 서버, 0: 클라이언트

# === 현재 경로 ===
CUR_PATH=$(pwd)
echo "현재 경로: $CUR_PATH"

# === 루트 경로 ===
ROOT_PATH=$(realpath "$PROJECT_PATH")
echo "루트 경로: $ROOT_PATH"

# === 프로토콜 경로 ===
PROTO_PATH="$ROOT_PATH/Common/Protocol"
if [ ! -d "$PROTO_PATH" ]; then
    echo "폴더 없음: $PROTO_PATH"
    exit 1
fi
echo "프로토콜 디렉토리: $PROTO_PATH"

# === 출력 경로 배열 ===
SERVER_OUT="$ROOT_PATH/Server/GameServer/Packet/Generated"
CLIENT_OUT="$ROOT_PATH/Client/Assets/@Scripts/Packet/Generated"

OUTPUT_PATHS=("$SERVER_OUT" "$CLIENT_OUT")

# 출력 폴더 없으면 생성
for OUT in "${OUTPUT_PATHS[@]}"; do
    if [ ! -d "$OUT" ]; then
        echo "출력 폴더 생성: $OUT"
        mkdir -p "$OUT"
    fi
done

# === protoc 실행 ===
cd "$PROTO_PATH" || { echo "cd 실패: $PROTO_PATH"; exit 1; }

echo "protoc 실행 중..."
for FILE in *.proto; do
    protoc -I=./ --csharp_out="$SERVER_OUT" "$FILE"
    protoc -I=./ --csharp_out="$CLIENT_OUT" "$FILE"
done

# === PacketGenerator 실행 (dotnet 사용) ===
PACKET_GEN="$ROOT_PATH/Tools/PacketGenerator/bin/PacketGenerator.dll"
if [ ! -f "$PACKET_GEN" ]; then
    echo "PacketGenerator.dll 없음: $PACKET_GEN"
    exit 1
fi

echo "PacketGenerator 실행 중..."
dotnet "$PACKET_GEN" -o "$SERVER_OUT" -t 1 -p "$PROTO_PATH/Protocol.proto"
dotnet "$PACKET_GEN" -o "$CLIENT_OUT" -t 0 -p "$PROTO_PATH/Protocol.proto"

# === 완료 ===
cd "$CUR_PATH"
echo "GenProto.sh 완료"
exit 0
