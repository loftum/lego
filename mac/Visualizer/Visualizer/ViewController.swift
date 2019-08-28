import Foundation
import Metal
import MetalKit

class ViewController: NSViewController {

    private let mtkView: MTKView
    private let renderer: Renderer

    init() {

        guard let device = MTLCreateSystemDefaultDevice() else {
            fatalError("Metal is not supported on this device")
        }
        print("GPU: \(device)")

        self.mtkView = MTKView(frame: CGRect(), device: device)
        self.renderer = Renderer(view: mtkView)
        mtkView.delegate = renderer
        super.init(nibName: nil, bundle: nil)
        self.view = mtkView
    }

    required init?(coder: NSCoder) {
        fatalError("init(coder:) has not been implemented")
    }
}