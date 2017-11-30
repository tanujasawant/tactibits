const int buffer = 1200; // Buffer size
int analogDataArray[buffer];
int count = 0;
int analogPin3 = A3; //x
int analogPin2 = A2; //y
int analogPin1 = A1; //z
int period = 1; // Period
unsigned long time1;
unsigned long time2;

void setup() {
  Serial.begin(9600);
  analogReference(EXTERNAL);
  
}
void loop() {
   analogDataArray[count++] = analogRead(analogPin3);
   analogDataArray[count++] = analogRead(analogPin2);
   analogDataArray[count++] = analogRead(analogPin1);
   
   if( micros()-time1 >= 100000){ //0.1s 
      for(int i=0;i<count;){
      Serial.print(analogDataArray[i++]);
      Serial.print("\t");
      Serial.print(analogDataArray[i++]);
      Serial.print("\t");
      Serial.print(analogDataArray[i++]);
      Serial.print("\n");
    }
    while(true);
   } 
}


