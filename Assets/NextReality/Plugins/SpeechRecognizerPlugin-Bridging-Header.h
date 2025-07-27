#import <Foundation/Foundation.h>

@interface SpeechRecognizerPlugin : NSObject
+ (instancetype)shared;
- (void)startRecognition:(void (^)(NSString *transcript))callback;
- (void)stopRecognition;
@end 