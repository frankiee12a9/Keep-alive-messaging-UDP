## Simple Keep-alive messaging communication using UDP protocol

### Requirements

-   Microsoft.NET Framework 4.5+

### Test

Clone the project and redirect to the solution

```
$ git clone git@github.com:frankiee12a9/Keep-alive-messaging-UDP.git

$ cd ./Keep-alive-messaging-UDP/KeepAliveMessaging
```

You can open a terminal for a server, and numerous other terminals for concurrent clients,
and compile them using `csc` command, which is Csharp compiler. For example:

#### Client

First terminal for client 1

```
$ csc Client.cs

$ ./Client.exe
```

![client1](https://user-images.githubusercontent.com/123849429/233013578-f6df371e-e8eb-422e-ba1a-566727dbfb88.png)

Second terminal for client 2

```
$ csc Client.cs

$ ./Client.exe
```

![client2](https://user-images.githubusercontent.com/123849429/233013828-5f1ca6ad-ed33-485b-aa14-04d2dad9a7c1.png)

Third terminal for client 3

```
$ csc Client.cs

$ ./Client.exe
```

![cilent3](https://user-images.githubusercontent.com/123849429/233014017-7835422c-f427-456e-8bf6-63074b2dad93.png)

#### Server

Terminal for a server

```bash
$ csc Server.cs

$ ./Server.exe
```

![server](https://user-images.githubusercontent.com/123849429/233014322-5f0b22bc-fb1c-4b98-b87f-6a199a1cb99e.png)

After connecting clients to a server successfully, while the keep-alive message is being sent, try to enter a separate message to change `UDP port`, `timeout`, `retry count` with following format:

```
# change udp port
port:<port_number>

# change timeout
timeout:<timeout_seconds>

# change retry count
retry:<count>
```

**If you seeing something like this after compiling the .cs files**

> error CS2012: Cannot open '\KeepAliveMessaging\KeepAliveMessaging\Client.exe' for writing -- 'The process cannot access the file '\KeepAliveMessaging\KeepAliveMessaging\Client.exe' because it is being used by another process.'

Just recompile it again

```bash
# client
$ csc Client.cs

# server
$ csc Server.cs
```

### Note

-   The project is developed using Console Application for the sake of simplicity
-   The project isn't completed yet (bugs might appear somewhere)
    -   the change udp port might cause the app crashes
