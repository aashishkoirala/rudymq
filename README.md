# [RudyMQ](http://aashishkoirala.github.io/rudymq/)
### A Rudimentary Message Queue for Windows
___
##### By [Aashish Koirala](http://aashishkoirala.github.io)

RudyMQ is a lightweight message queue for Windows built using .NET and WCF that you may want to consider if you don't need the enterprise grade features of MSMQ and don't want to deal with all of its tantrums and oh-so-friendly error messages. It supports persistent messages and also comes with its own WCF binding. 

#### Getting Started
+ To install or host the server:
 + Download, install and configure the server installation (see _Server Installation_), or:
 + Download the NuGet package [RudyMQ.Service](https://www.nuget.org/packages/RudyMQ.Service/) and add to your hosting application (see _Self Hosting_).
+ To talk to an existing instance:
 + Download the NuGet package [RudyMQ.Client](https://www.nuget.org/packages/RudyMQ.Client/) and add to your application
 + Access the queue API directly (see _Queue API_) or use the WCF binding provided (see _WCF Binding_)

#### Server Installation
+ Make sure .NET 4.5 is installed on your system.
+ Download the server installation [here](http://aashishkoirala.github.io/rudymq/downloads/RudyMQ.Server.zip).
+ Extract the files.
+ If you want to, make changes to the _AK.RudyMQ.Service.Host.exe.config_ file as needed. Instructions can be found inside the file itself.
+ Run _Install.cmd_.
+ A new service for RudyMQ should be registered which you can now start like any other service.

#### Self Hosting
+ Add the NuGet package [RudyMQ.Service](https://www.nuget.org/packages/RudyMQ.Service/) to your hosting application (which may be a web application, a Windows or console application, an Azure worker role - whatever).
+ Use the `QueueHost` class to instantiate and listen.
+ Example:

    	using (var queueHost = new QueueHost(hostName, port, baseAddress,
			catalogLocation, persistLocation, transitLocation, transitCleanupInterval, transitMaximumAge)
    	{
			queueHost.Open();
	
			// ...
		}

#### Queue API
+ Add the NuGet package [RudyMQ.Client](https://www.nuget.org/packages/RudyMQ.Client/) to your application.
+ You can send or receive typed messages. Message types can be any simple and serializable data structure. It needs to be marked as `Serializable`.
+ Sending a message:

		var conn = MessageQueue.Connect(hostName, port, baseAddress);
		var queue = conn.Get("MyQueue");
		
		var message = new MyMessage { ... };
		queue.Send(message);

+ Receiving a message:
 
		// Here, receiveOperation is IDisposable. You can call Stop on it, or just Dispose it,
		// use it inside a using block, whatever, when you want to stop receiving.
 
        var receiveOperation = queue.StartReceiving<MyMessage>(100, /* poll every 100 ms */
			message =>
            {
				// Handle received message here.

            }, exception =>
            {
				// Handle exception here.

            });
  
+ Other operations:
		
		conn.Create("MyQueue", true, false); // Create a new queue.
		conn.Remove("ExistingQueue"); // Remove an existing queue.

		queue.Purge<MyMessage>(); // Remove all messages of type MyMessage from the queue.
		queue.PurgeAll(); // Remove all messages from the queue.

#### WCF Binding
+ Just as with MSMQ, you can use RudyMQ as a transport mechanism for one-way WCF operations.
+ The binding is part of the [RudyMQ.Client](https://www.nuget.org/packages/RudyMQ.Client/) package.
+ The endpoint address is of the form **net.rudymq://**_hostname_**:** _port_**/**_baseAddress_**/**_queueName_
+ Example server side WCF configuration:

		<system.serviceModel>
			<extensions>
	  			<bindingExtensions>
	    			<add name="rudyMqBinding" type="AK.RudyMQ.Client.ServiceModel.RudyMqBindingSection, AK.RudyMQ.Client"/>
	  			</bindingExtensions>
			</extensions>
			<services>
	  			<service name="...">
	    			<endpoint address="net.rudymq://..." binding="rudyMqBinding" contract="..." />
		  		</service>
			</services>
		</system.serviceModel>


+ Example client side WCF configuration:

		<system.serviceModel>
			<extensions>
				<bindingExtensions>
					<add name="rudyMqBinding" type="AK.RudyMQ.Client.ServiceModel.RudyMqBindingSection, AK.RudyMQ.Client"/>
				</bindingExtensions>
			</extensions>
			<client>
				<endpoint address="net.rudymq://..." binding="rudyMqBinding" contract="..." />
			</client>
		</system.serviceModel>

+ Example server side when done programmatically:

        using (var serviceHost = new ServiceHost(...))
        {
            serviceHost.AddServiceEndpoint(..., new RudyMqBinding(), "net.rudymq://...");
            serviceHost.Open();
			...
		}

+ Example client side when done programmatically:

		var proxy = new _ServiceProxyClass_(new RudyMqBinding(), "net.rudymq://...");
		...
		proxy.Close();
