# AnnelidaDispatcher
Rule based many-to-many Protobuf dispatcher. Messages received are saved in a relevant MongoDatabase (disabled in this branch) and dispatched to listeners over the network.

The code relies on C# 5.0 async networking.

## Usage
TCP conections on on port 9999.
Before sending and receiving messages, the client needs to identify itself to the dispatcher.

The protocol buffer contracts for both robots and controllers are located in the `ProtocolBufferContracts` folder. Please see [Google's Protocol Buffers page](https://developers.google.com/protocol-buffers/) to learn how to use PB's in your programming language. 
**One very basic example in C# is located in the `ReceiveProtobuf` folder**

Because Protocol Buffers do not have end of stream markers the receivers should read the first 4 bytes (`Int32`) to know the message size. Senders should pack that information in the message.

### Client identification types:
Clients types are specified in their repesctive model class, however if you are connecting to the dispatcher with your own implementation make sure you follow the following represnetation:
* View: 1
* Controller: 2
* Robot: 3

### Sending messages to the dispatcher
The dispatcher will ignore any messages received from a client that has not yet identified itself. Therefore, the **first** thing to do it send the correspondend identification number.

After identification senders should serilize the protocol buffer object and add its size to the first 4 bytes of the outgoing message:

`message = [size,size,size,size,serialized protocol buffer]`

####Step-by-Step
1. Connect to dispatcher  using TCP on port 9999
2. Self-identify to the dispatcher. While the client is not identified all messages to it are dropped
   1. Send `byte[]` containing the clientType (`Int32`). Options are: 
      1. 1 - View
      2. 2 - Controller
      3. 3 - Robot
3. Dispatcher now is ready to include the client in the message cycle. 
4. Pack the message in the mentioned format and send to the dispacher.

### Reciving messages from the dispatcher
Clients willing to receive message should do as follows:
1. Connect to dispatcher  using TCP on port 9999
2. Self-identify to the dispatcher. While the client is not identified all messages to it are dropped
3. Start receiving messages from the dispatcher
   1. Read 4 bytes from the stream to know the lengh of the protocol buffer message
   2. Read the the size of the protocol buffer serialized message
   3. Convert the `byte[]` message back into a protocol buffer object
      * Do not include the first 4 bytes of the size into the protocol buffer deserialization!

## Remarks
Late connections are allowed however late clients will not receive any historical or digested data. 