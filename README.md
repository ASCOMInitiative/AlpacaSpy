## AlpacaSpy Summary
AlpacaSpy is a "man-in-the-middle" application that sits between an Alpaca client application and an Alpaca device. Its purpose is to log 
calls between the client and the device to aid debugging.

The AlpacaSpy setup dialogue can be used to configure AlpacaSpy to proxy up to 10 Alpaca devices. The proxy'd Alpaca devices are available to clients as normal disscoverable devices. The proxies have the same names as the original devices prefixed with the text "AlpacaSpy".

By default just the incoming parameters form the client and JSON response from the device are logged. However, the HTTP headers from both the client and device can also be logged and the JSON response can be broken down to its name:value pairs and listed in the log.

All ASCOM device types are supported and monitoring can be enabled / disabled on a member by member basis so that logging can be limited just to members of interest.

## Introduction

Analysing Alpaca client and device logs when debugging issues can be challenging due to differences in logging styles and completeness of logs. AlpacaSpy
aims to improve this by providing a mechanic to log commands and responses between an Alpaca client and a device.

AlpacaSpy is a cross-platform Blazor application with implementations for Windows, Linux x64, Linux Arm32, Linux Arm64 and MacOS. Its UI is delivered through the default browser.

## Capabilities
AlpacaSpy can:
* Act as a transparent intermediary between an Alpaca client and an Alpaca device that logs commands and responses for later analysis and debugging.
* Optionally record client commands for replay to the device at a later time to aid behavioural debugging.
* Compare device responses during playback with those originally recorded.
* Monitor many Alpaca connections simultaneously.
* Can run "headless" i.e. does not require a browser UI to be connected.

A core set of information is always logged for each transaction including

* The HTTP method.
* The ASCOM interface member being called.
* All client parameters
* The response HTTP status code.
* The JSON response in compact form when the HTTP response code is 200 OK.
* The text error message when the HTTP response code is not 200 OK.

## How to use AlpacaSpy

To insert AlpacaSpy between a client and a device:,

* Add the target device as a proxied device using the Setup page.
* Restart AlpacaSpy and ensure that the home page log says that the device is connected.
* Select the AlpacaSpy proxied device through the client's Alpaca Discovery or other device selection mechanic.
  The proxied device will appear with the device's normal name prefixed by with the text: "AlpacaSpy -".
* Client commands and device responses will appear on the AlpacaSpy home page and be recorded in the AlpacaSpy logs.

## Navigation

The navigation area to the left of the screen provides access to:

* Links to the Record, Playback and Setup pages.
* A button to Connect to / Disconnect from devices. (This is a "click - on, click - off" button.)
* Buttons to restart or shut down AlpacaSpy.

## Setup Page
#### Adding a device

In order to reduce screen clutter, by default, available Alpaca devices are not displayed on the Setup page. To add a device to AlpacaSpy:

* Ensure the Device State is: Disconnected.
* Click the Discover and select ALpaca devices button.
* Click the Add button for each device you want to include. This will add a device tab to the Settings section below.
* Configure the device settings if required.
* Restart AlpacaSpy, after which the device can be selected by the client application.

#### General Settings
There are cross-cutting settings for AlpacaSpy as a whole:
* Whether or not to connect to configured devices at startup.
* The IP port on which to operate (default 32325)
* The log level (Information / Debug etc.)
* The discovery response Location.
* Whether to log discovery diagnostics (not normally required).
* Whether to start a UI browser session when the application starts.
* The length of time to wait for devices to respond during discovery.
* The time before GET commands to the device time out.
* The time to wait for devices to connect.
* Whether to include debug trace information for communications with the device.
* Whether to bind to all network addresses or only to localhost.
* Whether to respond to Alpaca Discovery requests.
* Whether to respond to discovery requests on all network addresses or only localhost.
* Whether to accept incorrectly cased JSON responses from the device.
* The maximum number of client/device transactions to store for each configured device.

#### Device Settings

In addition to the core data, the following items can optionally be logged:

* Client headers sent to the device.
* Parameter names and values sent to the device.
* Headers send by the device to the client.
* Pretty-printed form of the JSON response sent by the device to the client.

## Notes
#### Playback

Slight timing differences between recording and playback may result in differences being reported in completion and status variables responses.
E.g. an asynchronous operation may complete slightly earlier or later on playback and completion variables such as "Telescope.Slewing" or status variables
such as "CoverCalibrator.CoverState" may consequently report spurious differences that can be ignored.

#### Headless Running

The AlpacaSpy executable will continue to run after all browser sessions are terminated. The best way to terminate the application
is by connecting a browser to the AlpacaSpy IP port (default 32325) and using the Shutdown button in the Navigation pane.
Alternatively, the "AlpacaSpy" process can be terminated through Task Manager.
