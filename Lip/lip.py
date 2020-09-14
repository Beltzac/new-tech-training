#!/usr/bin/env python
import pika, sys, os, requests, re, json, cgi

def downloadFile(id, folder):
    r = requests.get('http://file/api/File/' + id)  

    d = r.headers['content-disposition']

    value, params = cgi.parse_header(d)

    fname = params['filename']

    print(fname)

    location = folder + '/' + fname

    os.makedirs(folder, exist_ok=True)
    with open(location, 'wb') as f:
        f.write(r.content)
    
    return location

def uploadFile(id, folder):
    files = {'file': open(folder + '/' + id + '.mp4', 'rb')}
    r = requests.post('http://file/api/File/' + id, files=files)
    print(r.text)

def main():
    print("Iniciando...")
    connection = pika.BlockingConnection(pika.ConnectionParameters(host='queue', heartbeat=0)) #TODO: usar threads, reabilitar hearthbeat, dar ack apenas se funcionou
    channel = connection.channel()
    channel.basic_qos(prefetch_count=1) #como o processo é demorado pegamos um por um

    def updateStatus(ch, id, percentage, message):
        ch.basic_publish(exchange='status',
                         routing_key='',
                         body=json.dumps({"ProcessId": id, "PercentageProcessed": percentage, "Message": message}))

    def callback(ch, method, properties, body):
        print(" [x] Received %r" % body)

        data = json.loads(body)

        #send "begin processing" message
        updateStatus(ch, data['IdRequest'], 0, "Obtendo arquivos...")

        #get files
        image = downloadFile(data['IdImage'], 'input')
        updateStatus(ch, data['IdRequest'], 5, "Imagem adquirida")

        audio = downloadFile(data['IdAudio'], 'input')
        updateStatus(ch, data['IdRequest'], 10, "Audio adquirido")

        #make output
        os.makedirs('output', exist_ok=True)
        os.makedirs('temp', exist_ok=True)

        updateStatus(ch, data['IdRequest'], 20, "Processando")

        #call script
        script = "python Wav2Lip/inference.py --checkpoint_path Wav2Lip/checkpoints/wav2lip_gan.pth --face \"../" + image + "\" --audio \"../" + audio + "\" --outfile \"../output/" + data['IdRequest'] + ".mp4\""
        print(script)
        os.system(script)

        updateStatus(ch, data['IdRequest'], 80, "Processado, enviando resultado")

        #upload result
        uploadFile(data['IdRequest'], 'output')

        #send "end processing" message
        updateStatus(ch, data['IdRequest'], 100, "Finalizado!")


    channel.basic_consume(queue='lip', on_message_callback=callback, auto_ack=True)

    print(' [*] Waiting for messages. To exit press CTRL+C')
    channel.start_consuming()

if __name__ == '__main__':
    try:
        main()
    except KeyboardInterrupt:
        print('Interrupted')
        try:
            sys.exit(0)
        except SystemExit:
            os._exit(0)