#include "TinyWireS.h"
#define I2C_SLAVE_ADDR1 0x26

const int ledPin =  1;
const int vibPin = 7;
const int motor1Pin = 5;
const int motor2Pin = 8;
boolean running = false;
int motorIntensity;
int temperatureIntensity;
void setup() {
 pinMode(ledPin, OUTPUT); 
 pinMode(vibPin, OUTPUT); 
 pinMode(motor1Pin, OUTPUT); 
 pinMode(motor2Pin, OUTPUT); 
 TinyWireS.begin(I2C_SLAVE_ADDR1);
}

void loop() {
  if(TinyWireS.available()){
    motorIntensity=TinyWireS.receive();
    analogWrite(vibPin, motorIntensity);

    running=!running;
    digitalWrite(ledPin,running);
    
    temperatureIntensity=TinyWireS.receive();
    if(temperatureIntensity==32){
      analogWrite(motor2Pin, 255);
      digitalWrite(motor1Pin, LOW);
    }
    else{
      analogWrite(motor1Pin, temperatureIntensity);
      digitalWrite(motor2Pin, LOW);
    }
  } 
}
 

