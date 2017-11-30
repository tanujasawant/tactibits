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
    for(int j=0;j<m;j++){
      //Serial.println(j);
      //Serial.println(strArr[j]);
      str_long=strArr[j];
      slave=str_long.substring(0,2);
      str=str_long.substring(3);
      SLAVE_ADDR1=slave[0]-'0';
      SLAVE_ADDR1=(SLAVE_ADDR1*10)+(slave[1]-'0');
      //Serial.println(SLAVE_ADDR1);        
      Serial.println(str_long);
      //Serial.println(SLAVE_ADDR1);
       
   
     
      //Serial.println(str);
      /*
       * TODO: assign myArray[0] and myArray[1] values for LED, similar to V
       * 
      if(str=="LED BLINK"){
        Wire.beginTransmission(SLAVE_ADDR1);      
        Wire.write('1');
        Wire.endTransmission(); 
        delay(1000);
        Wire.beginTransmission(SLAVE_ADDR1);      
        Wire.write('0');
        Wire.endTransmission();                  
     }else if(str=="LED HIGH"){
        Wire.beginTransmission(SLAVE_ADDR1);      
        Wire.write('1');
        Wire.endTransmission();                  
      }else if(str=="LED LOW"){
        Wire.beginTransmission(SLAVE_ADDR1);      
        Wire.write('0');
        Wire.endTransmission();  */                
      }else if(str=="V HIGH"){
        myArray[0]=2;
        myArray[1]=1;
        Wire.beginTransmission(SLAVE_ADDR1);
        Wire.write(myArray[0]);
        Wire.endTransmission();
        Wire.beginTransmission(SLAVE_ADDR1);
        Wire.write(myArray[1]);
        Wire.endTransmission();
                          
      }else if(str=="V LOW"){
        myArray[0]=2;
        myArray[1]=0;
        Wire.beginTransmission(SLAVE_ADDR1);
        Wire.write(myArray[0]);
        Wire.endTransmission();
        Wire.beginTransmission(SLAVE_ADDR1);
        Wire.write(myArray[1]);
        Wire.endTransmission();
      }else if(str=="V BLINK"){
        myArray[0]=2;
        myArray[1]=0;
        for(int i=0;i<3;i++){
         Wire.beginTransmission(SLAVE_ADDR1);
         Wire.write(myArray[0]);
         Wire.endTransmission();
         Wire.beginTransmission(SLAVE_ADDR1);
         Wire.write(myArray[1]);
         Wire.endTransmission();
         delay(1000);
         myArray[1]=1;
         if(i==1)
          myArray[1]=0;
        }      
      }else if(str=="V ANALOG"){
         myArray[0]=1;
         myArray[1]=0;
         
        for(int i =0;i<102;i++){//102 because we want it to reach 255 only once in steps of 5, so 255/5=102 
          myArray[1] = myArray[1]+fadeAmount;
          //Serial.println(myArray[1]);
          Wire.beginTransmission(SLAVE_ADDR1);
          Wire.write(myArray[0]);
          Wire.endTransmission();
          Wire.beginTransmission(SLAVE_ADDR1);
          Wire.write(myArray[1]);
          Wire.endTransmission();
          delay(30);
          if(myArray[1]<=0 || myArray[1]>=255){
            fadeAmount=-fadeAmount;
          }
        }
       }
        
        /*this code is for debugging, ignore
         * 
         * myArray[1] = myArray[1]+fadeAmount;
        Serial.println(myArray[1]);
        Wire.beginTransmission(SLAVE_ADDR1);
         Wire.write(myArray[0]);
         Wire.endTransmission();
         Wire.beginTransmission(SLAVE_ADDR1);
         Wire.write(myArray[1]);
         Wire.endTransmission();
         delay(30);
         //Wire.requestFrom(I2C_SLAVE_ADDR1, 1);
         if(myArray[1]<=0 || myArray[1]>=255){
          fadeAmount=-fadeAmount;
         }
         counter++;
         if(counter==3000)
          exit(0);*/
   }
   while(true);
 }
