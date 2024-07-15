#import "mach/mach.h"
#import "UnityAppController.h"
#import "FCUUID.h"



@interface CGNative : NSObject {
    
}
#define MakeStringCopy( _x_ ) ( _x_ != NULL && [_x_ isKindOfClass:[NSString class]] ) ? strdup( [_x_ UTF8String] ) : NULL


@end
@implementation CGNative

@end

extern "C"
{
    void Alert(char *title, char *message)
    {
        NSString *t = [NSString stringWithUTF8String:title];
        NSString *m = [NSString stringWithUTF8String:message];
        UIAlertView *alert = [[UIAlertView alloc] initWithTitle:t
                                                        message:m
                                                       delegate:nil
                                              cancelButtonTitle:NSLocalizedString(@"OK", @"OK")
                                              otherButtonTitles:nil];
        [alert show];
    }
    
    // This is added for the Fyber iOS plugin.
    void SetUpCookieSettings()
    {
         NSLog(@"Setting cookie settings for Fyber");
        [NSHTTPCookieStorage sharedHTTPCookieStorage].cookieAcceptPolicy = NSHTTPCookieAcceptPolicyAlways;
    }
    
    const char* GetLocale()
    {
        NSUserDefaults* defs = [NSUserDefaults standardUserDefaults];
        NSArray* languages = [defs objectForKey:@"AppleLanguages"];
        NSString* preferredLang = [languages objectAtIndex:0];
        return strdup( [preferredLang UTF8String] );
    }
    
    BOOL IsValidCustomURL(char *customUrl)
    {
        NSString *t = [NSString stringWithUTF8String:customUrl];
        
        t = [@"" stringByAppendingString:t];
        NSURL *url = [NSURL URLWithString:t];
        if ([[UIApplication sharedApplication]canOpenURL:url])
            return TRUE;
        else
            return FALSE;
    }
    
    int GetDeviceMemoryInUse()
    {
        struct task_basic_info info;
        mach_msg_type_number_t size = sizeof(info);
        kern_return_t kerr = task_info(mach_task_self(),
                                       TASK_BASIC_INFO,
                                       (task_info_t)&info,
                                       &size);
        if( kerr == KERN_SUCCESS )
            return info.resident_size;
        
        return -1;
    }
    
    natural_t GetDeviceFreeMemory()
    {
        mach_port_t host_port;
        mach_msg_type_number_t host_size;
        vm_size_t pagesize;
        host_port = mach_host_self();
        host_size = sizeof(vm_statistics_data_t) / sizeof(integer_t);
        host_page_size(host_port, &pagesize);
        vm_statistics_data_t vm_stat;
        if (host_statistics(host_port, HOST_VM_INFO, (host_info_t)&vm_stat, &host_size) != KERN_SUCCESS)
        {
            NSLog(@"Failed to fetch vm statistics");
            return 0;
        }
        
        /* Stats in bytes */
        natural_t mem_free = vm_stat.free_count * pagesize;
        return mem_free;
    }
    
    //Total memory
    natural_t GetDeviceTotalMemory(void)
    {
        mach_port_t host_port = mach_host_self();
        mach_msg_type_number_t host_size = sizeof(vm_statistics_data_t) / sizeof(integer_t);
        vm_size_t pagesize;
        vm_statistics_data_t vm_stat;
        
        host_page_size(host_port, &pagesize);
        
        if (host_statistics(host_port, HOST_VM_INFO, (host_info_t)&vm_stat, &host_size) != KERN_SUCCESS) NSLog(@"Failed to fetch vm statistics");
        
        natural_t mem_used = (vm_stat.active_count + vm_stat.inactive_count + vm_stat.wire_count) * pagesize;
        natural_t mem_free = vm_stat.free_count * pagesize;
        natural_t mem_total = mem_used + mem_free;
        
        return mem_total;
    }
    
    float GetCPUUsage()
    {
        kern_return_t kr;
        task_info_data_t tinfo;
        mach_msg_type_number_t task_info_count;
        
        task_info_count = TASK_INFO_MAX;
        kr = task_info(mach_task_self(), TASK_BASIC_INFO, (task_info_t)tinfo, &task_info_count);
        if (kr != KERN_SUCCESS)
            return -1;
        
        task_basic_info_t      basic_info;
        thread_array_t         thread_list;
        mach_msg_type_number_t thread_count;
        
        thread_info_data_t     thinfo;
        mach_msg_type_number_t thread_info_count;
        
        thread_basic_info_t basic_info_th;
        uint32_t stat_thread = 0; // Mach threads
        
        basic_info = (task_basic_info_t)tinfo;
        
        // get threads in the task
        kr = task_threads(mach_task_self(), &thread_list, &thread_count);
        if (kr != KERN_SUCCESS)
            return -1;
        
        if (thread_count > 0)
            stat_thread += thread_count;
        
        long tot_sec = 0;
        long tot_usec = 0;
        float tot_cpu = 0;
        int j;
        
        for (j = 0; j < thread_count; j++)
        {
            thread_info_count = THREAD_INFO_MAX;
            kr = thread_info(thread_list[j], THREAD_BASIC_INFO,
                             (thread_info_t)thinfo, &thread_info_count);
            if (kr != KERN_SUCCESS)
                return -1;
            
            basic_info_th = (thread_basic_info_t)thinfo;
            
            if (!(basic_info_th->flags & TH_FLAGS_IDLE))
            {
                tot_sec = tot_sec + basic_info_th->user_time.seconds + basic_info_th->system_time.seconds;
                tot_usec = tot_usec + basic_info_th->system_time.microseconds + basic_info_th->system_time.microseconds;
                tot_cpu = tot_cpu + basic_info_th->cpu_usage / (float)TH_USAGE_SCALE * 100.0;
            }
            
        } // for each thread
        
        kr = vm_deallocate(mach_task_self(), (vm_offset_t)thread_list, thread_count * sizeof(thread_t));
        assert(kr == KERN_SUCCESS);
        
        return tot_cpu;
    }	
    
    const char* GetDeviceOSVersion()
    {
        NSString *currSysVer = [[UIDevice currentDevice] systemVersion];
        return MakeStringCopy(currSysVer);
    }

    const char* uuidForDevice()
    {
        NSString *uuid =[FCUUID uuidForDevice];
        NSLog(@"UUID from XCODE : %@", uuid);
        return MakeStringCopy(uuid);
    }
}
