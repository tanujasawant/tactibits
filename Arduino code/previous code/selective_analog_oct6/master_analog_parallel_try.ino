#include <Wire.h>
//#define SLAVE_ADDR1  0x26            // i2c slave address (38)

 int bigNum;
 byte myArray[2];
 int fadeAmount=5;
 String strArr[10];
 String temp;
 int m;
 String slave;
 String str_long;
 int counter=0;
 String str;
 int SLAVE_ADDR1;
 void setup() 
 {
     Wire.begin();
     Serial.begin(9600);
     myArray[0]=1;
     myArray[1]=0;
     m=0;
 }

 void loop() 
 {
    while(true && m<10){
      temp=Serial.readStringUntil('\n');
      if(temp.length()>0){
        if(temp=="END")
          break;
        strArr[m]=temp;
        Serial.println(strArr[m]);
        m++;
      }
    }
  
    Serial.println("DONE");
  //if(str=="V ANALOG"){
    myArray[0]=1;
    myArray[1]=0;
   for(int i =0;i<102;i++){//102 because we want it to reach 255 only once in steps of 5, so 255/5=102 
    myArray[1] = myArray[1]+fadeAmount;
    if(myArray[1]<=0 || myArray[1]>=255){
        fadeAmount=-fadeAmount;
    }
    for(int j=0;j<m;j++){
      str_long=strArr[j];
      slave=str_long.substring(0,2);
      str=str_long.substring(3);
      SLAVE_ADDR1=slave[0]-'0';
      SLAVE_ADDR1=(SLAVE_ADDR1*10)+(slave[1]-'0');
      Wire.beginTransmission(SLAVE_ADDR1);
      Wire.write(myArray[0]);
      Wire.endTransmission();
      Wire.beginTransmission(SLAVE_ADDR1);
      Wire.write(myArray[1]);
      Wire.endTransmission();
    }
    delay(30);
   }
   while(true);
 }
