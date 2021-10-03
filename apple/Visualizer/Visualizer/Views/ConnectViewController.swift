import Foundation
import AppKit

class ConnectViewController : NSViewController {

    private let hostField : NSTextField
    private let errorField: NSTextField
    private let connectButton: NSButton
    private let userDefaults: UserDefaults = .standard

    required init(coder: NSCoder) {
        fatalError("nibs not supported")
    }

    init() {
        hostField = NSTextField()
        hostField.stringValue = userDefaults.string(forKey: "host") ?? "host"
        hostField.isBordered = true

        connectButton = NSButton()
        connectButton.title = "Connect"
        if let cell = connectButton.cell {
            cell.isBezeled = true
        }
        errorField = NSTextField()
        errorField.textColor = .red
        errorField.isEditable = false
        errorField.isBordered = false
        errorField.backgroundColor = .clear
        super.init(nibName: nil, bundle: nil)

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
        super.viewDidLoad()
        connectButton.action = #selector(connectButtonClicked)
    }

    @objc
    func connectButtonClicked() {
        errorField.stringValue = ""
        if hostField.stringValue.isEmpty {
            return
        }
        let parts = hostField.stringValue.split(separator: ":")
        let host = parts[0]
        let port = parts.count > 1
                ? Int(parts[1]) ?? 5080
                : 5080

    }
}