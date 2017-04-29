# AnnelidaDispatcher
Rule based many-to-many BsonDocument dispatcher. Messages received are saved in a relevant MongoDatabase and dispatched to listeners over the network

## Usage
TCP conections on (for now) fixed port 9999.

Robot and Controllers should send a single message containing the serialized `BsonDocument`. Clients should read the first 4 bytes to know the total size of the message to alloc the necessary space in memory.

Send and Receive BSon folders contain the appropriate examples.

### Step-by-step
1. Connect to dispatcher  using TCP on port 9999
2. Self-identify to the dispatcher. While the client is not identified all messages to it are dropped
   1. Send `byte[]` containing the clientType (`Int32`). Options are: 
      1. 1 - View
      2. 2 - Controller
      3. 3 - Robot
3. Dispatcher now is ready to include the client in the message cycle. 
4. Message cycle begins:
    1. If view, client should start receiving the messages
    2. If controller or robot, client can send messages when necessary


## Remarks
Client disconnection is not yet implemented. Clients should not close connection but break the TCP pipe. Exception handling is done inside the code to treat this as a client disconnection.

Late connections are allowed however late clients will not receive any historical or digested data. 