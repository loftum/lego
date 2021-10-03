import Cocoa

class AppDelegate: NSObject, NSApplicationDelegate {

    var window: NSWindow!
    var controller : MainWindowController!

    func applicationDidFinishLaunching(_ aNotification: Notification) {
        controller = MainWindowController(window: window)
        // Insert code here to initialize your application
        window.contentRect(forFrameRect: CGRect(x: 0, y: 0, width: 1200, height: 1200))
    }

    func applicationWillTerminate(_ aNotification: Notification) {
        // Insert code here to tear down your application
    }

    func applicationSupportsSecureRestorableState(_ app: NSApplication) -> Bool {
        return true
    }


}

