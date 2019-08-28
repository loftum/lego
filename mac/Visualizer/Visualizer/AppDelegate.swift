import Cocoa

@NSApplicationMain
class AppDelegate: NSObject, NSApplicationDelegate {

    let windowController = MainWindowController()
    let window: NSWindow

    override init() {
        window = windowController.window!
    }

    func applicationDidFinishLaunching(_ aNotification: Notification) {
        window.makeKeyAndOrderFront(self)
    }

    func applicationWillTerminate(_ aNotification: Notification) {
        print("Terminating")
    }
}

