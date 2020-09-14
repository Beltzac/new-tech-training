FROM python:3.6 as base

ADD Wav2Lip /Wav2Lip

RUN apt-get update -y && apt-get install -y --no-install-recommends build-essential gcc libsndfile1 ffmpeg

RUN pip install pika
RUN pip install requests
RUN pip install -r Wav2Lip/requirements.txt
run pip install numba==0.48
RUN pip install tensorflow==1.5
RUN pip install librosa==0.7.2

#para não precisar recriar a imagem acima toda a hora
FROM base
ADD lip.py /
CMD [ "python", "-u", "./lip.py" ]