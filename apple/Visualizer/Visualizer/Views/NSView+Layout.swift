import Foundation
import AppKit

extension NSCoding where Self: NSView {

    func withSubview<T: NSView>(_ child: T, constraints: (T, Self) -> [NSLayoutConstraint]) -> Self {
        addSubview(child)
        child.translatesAutoresizingMaskIntoConstraints = false
        NSLayoutConstraint.activate(constraints(child, self))
        return self
    }
}