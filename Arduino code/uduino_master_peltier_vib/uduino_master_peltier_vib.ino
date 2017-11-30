#include <Wire.h>
#include<Uduino.h>
Uduino uduino("uduinoButton");

void setup()
{
  Wire.begin();                            // on rejoint le bus en tant que maitre
  Serial.begin(9600);
  uduino.addCommand("TriggerModule", f);
}

void f() {
  char *arg;
  arg = uduino.next();
  int moduleID = atoi(arg);
  arg = uduino.next();
  int motorIntensity = atoi(arg);
  arg = uduino.next();
  int temperatureIntensity = atoi(arg);
  //Serial.println("hehe");
  //Serial.println(moduleID);
  //Serial.println(motorIntensity);
  //Serial.println(temperatureIntensity);
  
  //master's code
    Wire.beginTransmission(moduleID);      
    Wire.write(motorIntensity);
    Wire.endTransmission();                  
    
    Wire.beginTransmission(moduleID);     
    Wire.write(temperatureIntensity);
    Wire.endTransmission();                  
 }

void loop()
{
  uduino.readSerial();
  delay(15);
}
