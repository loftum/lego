import Foundation
import MetalKit
import Metal

class CarRenderer : NSObject {

    let view: MTKView
    let stateProvider: ILegoCarStateProvider

    init(view: MTKView, stateProvider: ILegoCarStateProvider){
        self.view = view
        self.stateProvider = stateProvider
    }


}

extension CarRenderer : MTKViewDelegate {
    func mtkView(_ view: MTKView, drawableSizeWillChange size: CGSize) {

    }

    func draw(in view: MTKView) {

    }
}