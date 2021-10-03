import Foundation
import CoreGraphics
import AppKit

class MainWindowController : NSWindowController {

    private let connectViewController: ConnectViewController

    required init(coder: NSCoder) {
        fatalError("No nibs here")
    }

    init(window: NSWindow) {
        connectViewController = ConnectViewController()
        super.init(window: window)
        var frame = window.frame
        frame.size = NSSize(width: 1200, height: 1000)
        window.setFrame(frame, display: true)
        window.contentView = connectViewController.view
        connectViewController.delegate = self
    }
}

extension MainWindowController : ConnectViewControllerDelegate {

    func connectViewController(_ controller: ConnectViewController, didConnectTo host: String, port: Int) {
        print("click!")
    }
}