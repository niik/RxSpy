# RxSpy

Debugging Reactive applications can be hard™. Chaining and joining observables right and left often produce an observable graph that can be hard to comprehend.

RxSpy tries to solve or at least alleviate this problem by giving developers a live birds-eye view of the applications, all observables within it and the signals they produce. By referencing a small library your app you can launch a reactive debugging session of sorts which gives you a visual interface through which you can see all observables in your app, the values they produced and where they where created from.    

![rxspy-screenshot](https://f.cloud.github.com/assets/634063/2190610/ae6c0508-982d-11e3-90b3-bbb3ffbc2317.png)

RxSpy consists of two pieces. ```RxSpy``` which is the small library you include in your app and the visual tool RxSpy LiveView. ```RxSpy``` initiates a small server inside of your app which the visual tool can then connect to. Through that connection ```RxSpy.LiveView``` then streams all observable events that it can possibly get its hands on. You may also stream events to a file for analysis through some other tool. You do this by calling ```RxSpySession.Launch``` and passing in an instance of ```RxSpyStreamWriter```.

## WARNING WARNING BETA BETA

RxSpy is extremely young still. While the LiveView tool is suitable for demos, teaching etc it struggles to handle any real world application load. The reason is twofold, it's not been optimized yet and it's also really hard/impossible to take all events from multiple threads and try to serialize them onto the UI thread of the live view app in real time.

If you have a rought idea of what areas of your app you'd want to monitor you can use [explicit capture](https://github.com/niik/RxSpy/pull/24) to only look at a specific set of observables.

The long term plan is to build a companion app to the Live View tool that does non-realtime analysis of a captured file.

## Running it

The easiest way to start playing with RxSpy is to install the NuGet package RxSpy.LiveView into your app. Once you've done that or included RxSpy through other means call ```RxSpySession.Launch``` at the entry point of your application. This call will block until the UI has had a chance to launch and connect.

If you're not running it through nuget package you'll have to edit the ```RxSpySession.FindGuiPath``` and point it to the ```RxSpy.LiveView.exe```.

You can also clone RxSpy and try the RxSpy.TestConsole project.

## Things currently trackable

 - All observables created through one of the standard Rx operators.
   - The class and method from which the operator was created and (if available) the file name and line.   
 - Signals (values) produced by observables
   - The timestamp of when the signal was produced
   - A string representation of the value itself (using ```DebuggerDisplay``` if available, falls back to ```.ToString()```)
   - What thread the value was produced on (helpful for debugging UI thread issues)
 - If the observable completed in error and the full details of that error
 - Total number of observables observed (no pun intended)
 - Total number of signals so far in the app
 - Signals per second currently

## Nifty things

 - By double-clicking on an observable in the app you get a visual graph rendering of that observable, all of its ancestors and descendants. The graph is live-updating and 
 - If you tack on ```.SpyTag("Foo")``` to one of the observables in your app that tag will show in the UI. Making it easy to locate the observable in the app.
