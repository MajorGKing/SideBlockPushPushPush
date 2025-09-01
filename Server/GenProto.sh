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

# === 출력 경로 ===
OUTPUT_PATH=$(realpath "$ROOT_PATH/$OUTPUT_REL_PATH" 2>/dev/null)
echo "출력 경로: $OUTPUT_PATH"

# === 프로토콜 경로 ===
PROTO_PATH="$ROOT_PATH/Common/Protocol"
if [ ! -d "$PROTO_PATH" ]; then
    echo "폴더 없음: $PROTO_PATH"
    exit 1
fi
echo "프로토콜 디렉토리: $PROTO_PATH"

# === 출력 경로가 없으면 생성 ===
# 서버/클라이언트 경로 배열
OUTPUT_PATHS=(
    "$ROOT_PATH/Server/GameServer/Packet/Generated"
    "$ROOT_PATH/Client/Assets/@Scripts/Packet/Generated"
)

for OUT in "${OUTPUT_PATHS[@]}"; do
    if [ ! -d "$OUT" ]; then
        echo "출력 폴더 생성: $OUT"
        mkdir -p "$OUT"
    fi
done

# === protoc 실행 ===
cd "$PROTO_PATH" || { echo "cd 실패: $PROTO_PATH"; exit 1; }

echo "protoc 실행 중..."
protoc -I=./ --csharp_out="$OUTPUT_PATH" ./Protocol.proto ./Enum.proto ./Struct.proto
if [ $? -ne 0 ]; then
    echo "protoc 실행 실패"
    exit 1
fi

# === PacketGenerator 실행 ===
PACKET_GEN="$ROOT_PATH/Tools/PacketGenerator/bin/PacketGenerator.exe"
if [ ! -f "$PACKET_GEN" ]; then
    echo "PacketGenerator 없음: $PACKET_GEN"
    exit 1
fi

cd "$ROOT_PATH/Tools/PacketGenerator/bin" || { echo "cd 실패: $ROOT_PATH/Tools/PacketGenerator/bin"; exit 1; }

echo "PacketGenerator 실행 중..."
# 프로그램 타입에 따라 -t 옵션 전달
mono PacketGenerator.exe -o "$OUTPUT_REL_PATH" -t "$PROGRAM_TYPE"
if [ $? -ne 0 ]; then
    echo "PacketGenerator 실행 실패"
    exit 1
fi

# === 완료 ===
cd "$CUR_PATH"
echo "GenProto.sh 완료"
exit 0
