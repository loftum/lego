import Foundation
import AppKit
import MetalKit
import Metal

class VisualizerViewController: NSViewController {

    let client : LegoCarClient
    let mtkView: MTKView
    let renderer: CarRenderer
    var delegate: VisualizerViewControllerDelegate?

    required init(coder: NSCoder) {
        fatalError("No nibs here")
    }

    init() {
        guard let device = MTLCreateSystemDefaultDevice() else {
            fatalError("Metal not supported")
        }

        mtkView = MTKView(frame: CGRect(), device: device)
        mtkView.colorPixelFormat = .bgra8Unorm
        mtkView.depthStencilPixelFormat = .depth32Float

        client = LegoCarClient()
        renderer = CarRenderer(view: mtkView, stateProvider: client)
        super.init(nibName: nil, bundle: nil)

        let disconnectButton = NSButton(title: "Disconnect", target: self, action: #selector(disconnectClicked))

        view = NSView()
        .withSubview(mtkView, constraints: {c, p in [
            c.centerXAnchor.constraint(equalTo: p.centerXAnchor),
            c.centerYAnchor.constraint(equalTo: p.centerYAnchor),
            c.widthAnchor.constraint(equalTo: p.widthAnchor, multiplier: 0.8),
            c.heightAnchor.constraint(equalTo: p.heightAnchor, multiplier: 0.8)
        ]})
        .withSubview(disconnectButton, constraints: { c, p in [
            c.topAnchor.constraint(equalTo: p.topAnchor, constant: 20),
            c.centerXAnchor.constraint(equalTo: p.centerXAnchor)
        ]})
    }

    @objc
    func disconnectClicked() {
        print("Disconnect clicked")
        delegate?.visualizerViewController(self, disconnected: "by user")
    }

    func connect(host: String, port: Int) {

    }
}

protocol VisualizerViewControllerDelegate {
    func visualizerViewController(_ controller: VisualizerViewController, disconnected: String)
}
