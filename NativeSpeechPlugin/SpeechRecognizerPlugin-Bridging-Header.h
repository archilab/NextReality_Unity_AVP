#import <Foundation/Foundation.h>

@interface SpeechRecognizerPlugin : NSObject
+ (instancetype)sharedInstance;
- (void)startRecognition:(void (^)(NSString *transcript))callback;
- (void)stopRecognition;
@end 