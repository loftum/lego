import Foundation
import Cocoa

class MainWindowController: NSWindowController {

    private let vc = ViewController()

    init() {

        let frame = NSRect(x: 0, y: 0, width: 1000, height: 800)
        let styleMask = NSWindow.StyleMask.titled
                .union(.closable)
                .union(.miniaturizable)
                .union(.miniaturizable)
                .union(.resizable)
        let window = NSWindow(contentRect: frame, styleMask: styleMask, backing: .buffered, defer: false)
        window.title = "Visualizer"
        window.backgroundColor = .brown
        super.init(window: window)
        window.contentView = vc.view
        window.awakeFromNib()
    }

    required init?(coder: NSCoder) {
        fatalError("hest")
    }
}