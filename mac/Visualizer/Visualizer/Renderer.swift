import Foundation
import Metal
import MetalKit

class Renderer: NSObject, MTKViewDelegate {

    let device: MTLDevice
    let commandQueue: MTLCommandQueue
    let pipelineState: MTLRenderPipelineState
    let vertexBuffer: MTLBuffer
    var vertices: [Vertex]

    init(view: MTKView) {
        self.device = view.device!
        guard let commandQueue = device.makeCommandQueue() else {
            fatalError("Could not make commandQueue")
        }
        self.commandQueue = commandQueue
        do {
            pipelineState = try Renderer.buildRenderPipelineWith(device: device, metalKitView: view)
        }
        catch {
            fatalError("\(error)")
        }

        vertices = [Vertex(color: [1, 0, 0, 1], pos: [-1, -1]),
                        Vertex(color: [0, 1, 0, 1], pos: [0, 1]),
                        Vertex(color: [0, 0, 1, 1], pos: [1, -1])]
        guard let buffer = device.makeBuffer(bytes: vertices, length: vertices.count * MemoryLayout<Vertex>.stride, options: []) else {
            fatalError("Could not make buffer")
        }
        vertexBuffer = buffer
    }

    class func buildRenderPipelineWith(device: MTLDevice, metalKitView: MTKView) throws -> MTLRenderPipelineState {
        let pipelineDescriptor = MTLRenderPipelineDescriptor()
        guard let library = device.makeDefaultLibrary() else {
            fatalError("Could not makeDefaultLibrary")
        }
        pipelineDescriptor.vertexFunction = library.makeFunction(name: "vertexShader")
        pipelineDescriptor.fragmentFunction = library.makeFunction(name: "fragmentShader")
        pipelineDescriptor.colorAttachments[0].pixelFormat = metalKitView.colorPixelFormat
        return try device.makeRenderPipelineState(descriptor: pipelineDescriptor)
    }

    func mtkView(_ view: MTKView, drawableSizeWillChange size: CGSize) {

    }

    func draw(in view: MTKView) {

        for ii in 0..<vertices.count {
            var vertex = vertices[ii]
            print("hello \(vertex.pos)")
            var x = vertex.pos.x
            var y = vertex.pos.y
            if x <= -1 {
                x = -1
                if y < 1 {
                    y += 0.01
                }
                else {
                    x += 0.01
                }
            }
            else if y >= 1 {
                y = 1
                if x < 1 {
                    x += 0.01
                }
                else {
                    y -= 0.01
                }
            }
            else if x >= 1 {
                x = 1
                if y > -1 {
                    y -= 0.01
                }
                else {
                    x -= 0.01
                }
            }
            else if y <= -1 {
                y = -1
                if x > -1 {
                    x -= 0.01
                }
                else {
                    y += 0.01
                }
            }

            vertex.pos = [x, y]
            vertices[ii] = vertex
            print("\(vertex.pos)")
        }

        guard let commandBuffer = commandQueue.makeCommandBuffer(),
              let renderPassDescriptor = view.currentRenderPassDescriptor else {
            return
        }
        let color = MTLClearColor(red: 0, green: 0, blue: 0, alpha: 1)
        renderPassDescriptor.colorAttachments[0].clearColor = color

        guard let renderEncoder = commandBuffer.makeRenderCommandEncoder(descriptor: renderPassDescriptor) else {
            return
        }


        renderEncoder.setRenderPipelineState(pipelineState)
        renderEncoder.setVertexBuffer(vertexBuffer, offset: 0, index: 0)
        renderEncoder.drawPrimitives(type: .triangle, vertexStart: 0, vertexCount: 3)

        renderEncoder.endEncoding()
        guard let drawable = view.currentDrawable else {
            return
        }

        commandBuffer.present(drawable)
        commandBuffer.commit()
    }
}