import Foundation
import AppKit

class ConnectViewController : NSViewController {

    private let hostField : NSTextField
    private let errorField: NSTextField
    private let userDefaults: UserDefaults = .standard
    var delegate: ConnectViewControllerDelegate?

    required init(coder: NSCoder) {
        fatalError("nibs not supported")
    }

    init() {
        hostField = NSTextField()
        hostField.stringValue = userDefaults.string(forKey: "host") ?? "host"
        hostField.isBordered = true


        errorField = NSTextField()
        errorField.textColor = .red
        errorField.isEditable = false
        errorField.isBordered = false
        errorField.backgroundColor = .clear
        super.init(nibName: nil, bundle: nil)

        let connectButton = NSButton(title: "Connect", target: self, action: #selector(connectButtonClicked))
        if let cell = connectButton.cell {
            cell.isBezeled = true
        }

        view = NSView()
        .withSubview(hostField, constraints: { c, p in [
            c.trailingAnchor.constraint(equalTo: p.centerXAnchor, constant: -10),
            c.centerYAnchor.constraint(equalTo: p.centerYAnchor),
            c.widthAnchor.constraint(greaterThanOrEqualToConstant: 200)
        ]})
        .withSubview(connectButton, constraints: {c, p in [
            c.leadingAnchor.constraint(equalTo: p.centerXAnchor, constant: 10),
            c.centerYAnchor.constraint(equalTo: p.centerYAnchor)
        ]})
        .withSubview(errorField, constraints: {c, p in [
            c.centerXAnchor.constraint(equalTo: p.centerXAnchor),
            c.topAnchor.constraint(equalTo: hostField.bottomAnchor, constant: 20),
            c.widthAnchor.constraint(greaterThanOrEqualToConstant: 400)
        ]})
    }

    override func viewDidLoad() {
        print("viewDidLoad")
        super.viewDidLoad()
    }

    @objc
    func connectButtonClicked() {
        print("connectButtonClicked")
        errorField.stringValue = ""
        if hostField.stringValue.isEmpty {
            return
        }
        let parts = hostField.stringValue.split(separator: ":")
        let host = String(parts[0])
        let port = parts.count > 1
                ? Int(parts[1]) ?? 5080
                : 5080

        self.delegate?.connectViewController(self, didConnectTo: host, port: port)
    }
}

protocol ConnectViewControllerDelegate {
    func connectViewController(_ controller: ConnectViewController, didConnectTo host: String, port: Int)
}