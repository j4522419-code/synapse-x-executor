"""Brand the two identification aliases and the default HTTP User-Agent."""
import hashlib,struct

NAME='SYNAPSE X MADE BY GRAND'
VERSION='1.3.4-grand'
USER_AGENT=NAME+'/'+VERSION
IDENTIFY_RVA=0x2439A0
IDENTIFY_END=0x243A97
PUSH_STRING_RVA=0x158020
STRLEN_RVA=0x38D49C
UA_RVA=0x246871
UA_END=0x2468F6
STRING_CONSTRUCT_RVA=0x72720

def patch_identity(data,pe):
    changes=[]
    def region(start,end):
        off=pe.get_offset_from_rva(start);return bytes(data[off:off+end-start])
    def replace(rva,end,code,purpose):
        assert len(code)<=end-rva
        after=bytes(code).ljust(end-rva,b'\x90');off=pe.get_offset_from_rva(rva)
        before=bytes(data[off:off+len(after)]);data[off:off+len(after)]=after
        changes.append({'rva':hex(rva),'before':before.hex(),'after':after.hex(),'purpose':purpose})
    def call(code,start,target):code+=b'\xe8'+struct.pack('<i',target-(start+len(code)+5))
    def padded(text):
        value=text.encode('ascii');return value+b'\0'*((-len(value)-1)%8+1)
    assert hashlib.sha256(region(IDENTIFY_RVA,IDENTIFY_END)).hexdigest()=='26000683039681f87ee73558656964b487d6300268122d296ae4ef97b6946358'
    assert hashlib.sha256(region(UA_RVA,UA_END)).hexdigest()=='f17de0e58a331304e21ce09d217d8623517903b0b5645c7d54c4bfae9b2b696f'
    # Both identifyexecutor and getexecutorname point at this same function.
    ref=pe.get_offset_from_rva(0x69C52)
    assert data[ref:ref+3]==bytes.fromhex('488d1d')
    assert 0x69C59+struct.unpack_from('<i',data,ref+3)[0]==IDENTIFY_RVA
    # Retain the original prologue, stack frame and saved registers. The old
    # 16-byte name buffer was too small for the full requested name; this uses
    # the existing 32-byte scratch space and explicit Lua string lengths.
    start=IDENTIFY_RVA+13
    assert region(IDENTIFY_RVA,start)==bytes.fromhex('48895c2410574883ec50488bf9')
    code=bytearray()
    for text in [NAME,VERSION]:
        value=padded(text);assert len(value)<=32
        for index in range(0,len(value),8):
            code+=bytes.fromhex('48b8')+value[index:index+8]+bytes.fromhex('48894424')+bytes([0x20+index])
        code+=bytes.fromhex('488d54242041b8')+struct.pack('<I',len(text))+bytes.fromhex('488bcf')
        call(code,start,PUSH_STRING_RVA)
    code+=bytes.fromhex('b802000000488b5c24684883c4505fc3')
    replace(start,IDENTIFY_END,code,'Full identification name/version using the existing Lua helper, frame and epilogue')
    # HTTP requests previously constructed Potassium/<backend version> here.
    # Keep RSI/RDI/R13 exactly as before: later code reuses those constants for
    # protocol header keys. Change only the User-Agent value, leaving headers,
    # fingerprint values, request routing, authentication and transport intact.
    code=bytearray(bytes.fromhex('48be')+struct.pack('<Q',0xcdeeb40c97c06979))
    code+=bytes.fromhex('48bf')+struct.pack('<Q',0xb887c77ff6b40629)
    code+=bytes.fromhex('49bd')+struct.pack('<Q',0x38e285fab46512f6)
    value=padded(USER_AGENT);assert len(value)<=48
    for index in range(0,len(value),8):
        code+=bytes.fromhex('48b8')+value[index:index+8]+bytes.fromhex('488945')+bytes([0x40+index])
    code+=bytes.fromhex('488d4d80488d554041b8')+struct.pack('<I',len(USER_AGENT))
    call(code,UA_RVA,STRING_CONSTRUCT_RVA)
    code+=bytes.fromhex('488d4580')
    replace(UA_RVA,UA_END,code,'Default HTTP User-Agent value, using the original std::string constructor and cleanup')
    return changes
