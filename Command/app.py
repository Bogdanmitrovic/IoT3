from flask import Flask,render_template,request
from flask_socketio import SocketIO, emit
from flask_mqtt import Mqtt
from flask_cors import CORS
import subprocess

#import eventlet
#eventlet.monkey_patch()

app = Flask(__name__)
CORS(app,resources={r"/*":{"origins":"*"}})
socketio = SocketIO(app,debug=True,cors_allowed_origins='*')#,async_mode='eventlet')

app.config['MQTT_BROKER_URL'] = 'mosquitto'
app.config['MQTT_BROKER_PORT'] = 1883
app.config['MQTT_USERNAME'] = ''
app.config['MQTT_PASSWORD'] = ''
app.config['MQTT_KEEPALIVE'] = 5
app.config['MQTT_TLS_ENABLED'] = False
topic = 'jammedJunction'

mqtt_client = Mqtt(app)

@app.route('/')
def home():
    return render_template('base.html')

@socketio.on('connect', namespace='/')
def connect():
    print('Client connected')

@mqtt_client.on_connect()
def handle_connect(client, userdata, flags, rc):
   if rc == 0:
       print('Connected successfully')
       mqtt_client.subscribe(topic)
   else:
       print('Bad connection. Code:', rc)

@mqtt_client.on_message()
def handle_mqtt_message(client, userdata, message):
   data = dict(
       topic=message.topic,
       payload=message.payload.decode()
  )
   print('Received message on topic: {topic} with payload: {payload}'.format(**data))
   socketio.emit('',message)

if __name__ == '__main__':
   #app.run(host='127.0.0.1', port=5000)
   #eventlet.wsgi.server(eventlet.listen(('localhost', 5000)), app)
   socketio.run(app, debug=True, port=5001, allow_unsafe_werkzeug=True)