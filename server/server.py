# Note - please never actually use WebSockets for desktop applications like this

from twisted.python import log
from twisted.internet import reactor
from autobahn.twisted.websocket import WebSocketServerProtocol
from autobahn.twisted.websocket import WebSocketServerFactory

gameclient = None
phoneclients = []

class MyServerProtocol(WebSocketServerProtocol):

    def onMessage(self, payload, isBinary):
        global gameclient
        if (isBinary):
            payload = payload.encode()
        print(payload)

        if (payload == 'r'): # game client
            print("Helo game client")
            gameclient = self
            for pc in phoneclients:
                pc.sendMessage('c')
        elif (payload == 'p'): # phone client
            phoneclients.append(self)
            if not(gameclient is None):
                print("Let the phone know te client exists")
                self.sendMessage('c')
        elif ((payload == 'g' or payload == 's') and self == gameclient):
            print("Game client said something")
            for pc in phoneclients:
                pc.sendMessage(payload)
        else:
            gameclient.sendMessage(payload)
            

    def onOpen(self):
        print("Client connected")


    def onClose(self, wasClean, code, reason):
        global gameclient
        if (self in phoneclients):
            phoneclients.remove(self)
        elif (self == gameclient):
            gameclient = None
            print("Goodbye game client")
            for pc in phoneclients:
                pc.sendMessage('q')

if __name__ == '__main__':
    factory = WebSocketServerFactory("ws://127.0.0.1:9001",debug=True)
    factory.protocol = MyServerProtocol
    reactor.listenTCP(9001,factory)
    reactor.run()
