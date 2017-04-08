# AnnelidaDispatcher
Rule based many-to-many BsonDocument dispatcher. Messages received are saved in a relevant MongoDatabase and dispatched to listeners over the network

## Usage
TCP conections on (for now) fixed port 9999.
Messages should be send in two stages. First one is an Int32 containing the size of the BsonDocument byte array. Second message is the serialized BsonDocument.

### Step-by-step
1. Connect to dispatcher  using TCP on port 9999
2. Self-identify to the dispatcher. While the client is not identified all messages to it are dropped
   1. Send `byte[]` containing 4 (`Int32`)
   2. Send `byte[]` containing the clientType (`Int32`). Options are: 
      1. 1 - View
      2. 2 - Controller
      3. 3 - Robot
3. Dispatcher now is ready to include the client in the message cycle. 

### Messages to view and robot clients
Views and robots shall receive serialized `BsonDocuments` in two steps. First a serialized `Int32` is sent containing the `BsonDocument` size then the document itself is sent

### Messages from controller and robot clients
Controllers and robots should send serialized must send serialized `BsonDocuments` in two steps. First a serialized `Int32` containing the `BsonDocument` size then the document itself.

Messages sent my theese clients are saved in their respective database in parallel to the dispatch cycle.

## Remarks
Client disconnection is not yet implemented. Clients should not close connection but break the TCP pipe. Exception handling is done inside the code to treat this as a client disconnection.