### AlpacaSpy
AlpacaSpy is a "man-in-the-middle" application that sits between an Alpaca client application and an Alpaca device. Its purpose is to log 
calls between the client and the device to aid debugging.

The AlpacaSpy setup dialogue can be used to configure AlpacaSpy to proxy up to 10 Alpaca devices. The proxy'd Alpaca devices are available to clients as normal disscoverable devices. The proxies have the same names as the original devices prefixed with the text "AlpacaSpy".

By default just the incoming parameters form the client and JSON response from the device are logged. However, the HTTP headers from both the client and device can also be logged and the JSON response can be broken down to its name:value pairs and listed in the log.

All ASCOM device types are supported and monitoring can be enabled / disabled on a member by member basis so that logging can be limited just to members of interest.
