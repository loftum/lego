import Foundation
import CoreGraphics
import AppKit

class MainWindowController : NSWindowController {

    private let connectViewController: ConnectViewController
    private let visualizerViewController: VisualizerViewController
    required init(coder: NSCoder) {
        fatalError("No nibs here")
    }

    init(window: NSWindow) {
        connectViewController = ConnectViewController()
        visualizerViewController = VisualizerViewController()
        super.init(window: window)
        var frame = window.frame
        frame.size = NSSize(width: 1200, height: 1000)
        window.setFrame(frame, display: true)
        window.contentView = connectViewController.view
        connectViewController.delegate = self
        visualizerViewController.delegate = self
    }
}

extension MainWindowController : ConnectViewControllerDelegate {

    func connectViewController(_ controller: ConnectViewController, didConnectTo host: String, port: Int) {
        window?.contentView = visualizerViewController.view
    }
}

extension MainWindowController: VisualizerViewControllerDelegate {

    func visualizerViewController(_ controller: VisualizerViewController, disconnected: String) {
        print("alright!")
        window?.contentView = connectViewController.view
    }
}