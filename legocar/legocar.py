import piconzero as pz, time, hcsr04, thread

import sys
import tty
import termios

def readchar():
    fd = sys.stdin.fileno()
    old_settings = termios.tcgetattr(fd)
    try:
        tty.setraw(sys.stdin.fileno())
        ch = sys.stdin.read(1)
    finally:
        termios.tcsetattr(fd, termios.TCSADRAIN, old_settings)
    if ch == '0x03':
        raise KeyboardInterrupt
    return ch

def readkey(getchar_fn=None):
    getchar = getchar_fn or readchar
    c1 = getchar()
    if ord(c1) != 0x1b:
        return c1
    c2 = getchar()
    if ord(c2) != 0x5b:
        return c1
    c3 = getchar()
    return chr(0x10 + ord(c3) - 65)  # 16=Up, 17=Down, 18=Right, 19=Left arrows


print "Up, Down, Left, Right to drive car"
print "Press <space> key to centre"
print "Press Ctrl-C to end"
print

class Car:
    steerPin = 0
    motorPin = 1
    steer = 90
    speed = 0
    distance = 0
    running = True
    
    def speedUp(self):
        if self.speed < 0:
            self.stop()
        else:
            self.setSpeed(self.speed + 30)
    
    def speedDown(self):
        if self.speed > 0:
            self.stop()
        else:
            self.setSpeed(self.speed - 30)
    
    def measureDistance(self):
        self.distance = hcsr04.getDistance()
        if self.distance < 15 and self.speed > 0:
            print 'Obstacle! ', self.distance
            self.stop()

    def steerLeft(self):
        self.setSteer(self.steer + 10)

    def steerRight(self):
        self.setSteer(self.steer - 10)

    def setSteer(self, steer):
        if steer > 145 or steer < 35:
            return
        self.steer = steer
        pz.setOutput(self.steerPin, self.steer)
        print 'Steer', self.steer

    def reset(self):
        self.setSteer(90)
        self.setSpeed(0)
        print 'Steer', self.steer
        print 'Speed', self.speed

    def setSpeed(self, speed):
        if speed > 127 or speed < -127:
            return
        if speed > 0 and self.distance < 15:
            self.setSpeed(0)
            return
        self.speed = speed
        pz.setMotor(self.motorPin, self.speed)
        print 'Speed', self.speed

    def stop(self):
        self.setSpeed(0)

car = Car()
hcsr04.init()
pz.init()
pz.setOutputConfig(car.steerPin, 2)
pz.setOutput (car.steerPin, car.steer)

def stopIfObstacle(car, seconds):
    while car.running:
        try:
            car.measureDistance()
        finally:
            time.sleep(seconds)
        
# main loop
try:
    
    thread.start_new_thread(stopIfObstacle, (car, 0.2,))
    while True:
        keyp = readkey()
        print keyp
        if ord(keyp) == 19: # left
            car.steerLeft()
        elif ord(keyp) == 18: # right
            car.steerRight()
        elif ord(keyp) == 16: # up
            car.speedUp()
        elif ord(keyp) == 17: # down
            car.speedDown()
        elif keyp == ' ':
            car.reset()
        elif keyp == 'd':
            print 'Distance: ', car.distance
        elif ord(keyp) == 3:
            print 'Done!'
            break

except KeyboardInterrupt:
    running = False
    print

finally:
    runnint = False
    pz.cleanup()
    hcsr04.cleanup()