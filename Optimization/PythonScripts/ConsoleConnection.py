from Connector import Connector

path_to_images = r"C:\Users\Public\Pictures\Sample Pictures"

#  Socket to talk to server
print("Connecting to hello world server")
#socket.connect("ipc:///path")
running = True
request = 0
conn = Connector()
#  Do 10 requests, waiting each time for a response

while running:
        req_msg = raw_input("Write your request\n")
        #req_msg = str(req_msg)
        
        if req_msg == "exit":
                running = False
                continue
        if(req_msg != "poll"):
                message = conn.send_message(req_msg, path_to_images)
        else:
                resp = conn.receive_message()
                print(resp)
    
        print("Received reply %s [ %s ]" % (request, message))
        request = request + 1
