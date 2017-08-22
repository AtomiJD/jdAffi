#include <Wire.h>  
#include <Servo.h>

#define SLAVE_ADDRESS 0x40   
#define SERVO_UP 30
#define SERVO_DOWN 90

#define TRIM_DURATION 1

int sensorPin01 = A1;
int sensorPin02 = A2;
int sensorPin03 = A3; // Kannensensor
int ts = 0;
int sensorValue = 0;
int relayPin = 3;
int fillmode = 0;

int i = 0;
int v = 0;
int pos = 0;
int wastat = 0;

Servo myservo;  

void setup()
{
  Serial.begin(9600);
  pinMode(LED_BUILTIN, OUTPUT);
  pinMode(relayPin, OUTPUT);
  myservo.attach(9);
  Wire.begin(SLAVE_ADDRESS); 
  Wire.onReceive(receiveData); 
  Wire.onRequest(sendData); 
}

void loop()
{
  Serial.print("Hi jd: ");
  digitalWrite(LED_BUILTIN, HIGH);  
  if (wastat == 1)
  {
       pos += 2;
       Serial.print("P: "); 
       Serial.println(pos); 
       myservo.write(pos);
       delay(40);
       if (pos > 140) wastat = 0;
  } else
  {
    delay(1000); 
  }
  ts = normalValue(analogRead(sensorPin01));
  sensorValue = ts;
  ts = normalValue(analogRead(sensorPin02));
  sensorValue = sensorValue + (ts * 10);
  ts = normalValue(analogRead(sensorPin03));
  sensorValue = sensorValue + (ts * 100);
  if (fillmode == 1) {
    if (ts == 3) {
       digitalWrite(relayPin, LOW);
       fillmode = 0;
    }
  }
  Serial.println(sensorValue);
  digitalWrite(LED_BUILTIN, LOW);   
  if (wastat == 0)  delay(1000);
}

void sendData()
{
  char buf [5];
  sprintf (buf, "%05i", sensorValue);
  Wire.write(buf); 
}

void receiveData(int byteCount)
{
  while(Wire.available()) 
  {
    v = Wire.read();
    Serial.print("data received: ");
    Serial.println(v);
    if (v == 1)
    {
      digitalWrite(LED_BUILTIN, HIGH);
      wastat = 1;
      pos = myservo.read();
    } else if (v == 2)
    {
      digitalWrite(LED_BUILTIN, LOW);
      //myservo.write(SERVO_DOWN);
      myservo.write(90);
      delay(15);    
      digitalWrite(relayPin, LOW);
    }
    else if (v == 3)
    {
      digitalWrite(relayPin, HIGH);
      fillmode = 1;
    }
  }
}

int normalValue(int v)
{
  int n = 0;
  Serial.println(v);
  if (v > 1000) n = 1;
  if ((v > 700) && (v <= 1000)) n = 2;
  if (v < 700) n = 3;
  return n;
}

