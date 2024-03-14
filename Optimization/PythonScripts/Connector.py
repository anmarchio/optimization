'''
example class for connecting to other programs via the local tcp socket.
for other connection options see:

should work identically.

this simple implementation cannot handle multiple requests at once.
it is the job of the calling application to schedule multiple requests, manage multiple
different connectors and so on.
'''
import zmq
import time
class Connector:

        def __init__(self, endpoint = None):
                if endpoint is None:
                        self.endpoint = "tcp://localhost:5555"
                else:
                        self.endpoint = endpoint
                self.context = zmq.Context()
                self.socket = self.context.socket(zmq.REQ)
                self.socket.connect(self.endpoint)
                self.running = True

        def interrupt(self):
                self.running = False
        
        def send_message(self, job, data):
                if ';' in job or ';' in data:
                        raise ValueError("neither job nor data must contain ';'")
                
                self.socket.send(job + ';' + data)


        def receive_message(self):
                if self.running:
                        try:
                                response = self.socket.recv(0) # 0 == NOBLOCK
                        except zmq.ZMQError:
                                time.sleep(1)
                        else:
                                response = "Connection Interrupted"

                return response
                
	
