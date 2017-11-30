//#include <usiTwiSlave.h>
#include "TinyWireS.h"
#define I2C_SLAVE_ADDR 0x26

const int ledPin =  1;
byte a,b;
int num1;
int num2;
void setup() {
  pinMode(ledPin, OUTPUT);
//  pinMode(vibPin, OUTPUT);
  TinyWireS.begin(I2C_SLAVE_ADDR);
}


void loop() {
  if(TinyWireS.available()){
   a=TinyWireS.receive();
   b=TinyWireS.receive();
  num1=a;
  num2=b;
    
    if(num1==1)
     analogWrite(ledPin, num2);
    else if(num1==2){
      if(num2==1)
        digitalWrite(ledPin,HIGH);
    else if(num2==0)
        digitalWrite(ledPin,LOW);
    }
}
}
