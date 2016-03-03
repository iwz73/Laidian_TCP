// Generated by ProtoGen, Version=2.4.1.555, Culture=neutral, PublicKeyToken=17b3b1f090c3ea48.  DO NOT EDIT!
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.ProtocolBuffers;
using pbc = global::Google.ProtocolBuffers.Collections;
using pbd = global::Google.ProtocolBuffers.Descriptors;
using scg = global::System.Collections.Generic;
namespace google.protobuf {
  
  namespace Proto {
    
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    public static partial class Any {
    
      #region Extension registration
      public static void RegisterAllExtensions(pb::ExtensionRegistry registry) {
      }
      #endregion
      #region Static variables
      internal static pbd::MessageDescriptor internal__static_google_protobuf_Any__Descriptor;
      internal static pb::FieldAccess.FieldAccessorTable<global::google.protobuf.Any, global::google.protobuf.Any.Builder> internal__static_google_protobuf_Any__FieldAccessorTable;
      #endregion
      #region Descriptor
      public static pbd::FileDescriptor Descriptor {
        get { return descriptor; }
      }
      private static pbd::FileDescriptor descriptor;
      
      static Any() {
        byte[] descriptorData = global::System.Convert.FromBase64String(
            string.Concat(
              "Chlnb29nbGUvcHJvdG9idWYvYW55LnByb3RvEg9nb29nbGUucHJvdG9idWYi", 
              "NgoDQW55EhkKCHR5cGVfdXJsGAEgASgJUgd0eXBlVXJsEhQKBXZhbHVlGAIg", 
              "ASgMUgV2YWx1ZUJLChNjb20uZ29vZ2xlLnByb3RvYnVmQghBbnlQcm90b1AB", 
              "oAEBogIDR1BCqgIeR29vZ2xlLlByb3RvYnVmLldlbGxLbm93blR5cGVzYgZw", 
            "cm90bzM="));
        pbd::FileDescriptor.InternalDescriptorAssigner assigner = delegate(pbd::FileDescriptor root) {
          descriptor = root;
          internal__static_google_protobuf_Any__Descriptor = Descriptor.MessageTypes[0];
          internal__static_google_protobuf_Any__FieldAccessorTable = 
              new pb::FieldAccess.FieldAccessorTable<global::google.protobuf.Any, global::google.protobuf.Any.Builder>(internal__static_google_protobuf_Any__Descriptor,
                  new string[] { "TypeUrl", "Value", });
          pb::ExtensionRegistry registry = pb::ExtensionRegistry.CreateInstance();
          RegisterAllExtensions(registry);
          return registry;
        };
        pbd::FileDescriptor.InternalBuildGeneratedFileFrom(descriptorData,
            new pbd::FileDescriptor[] {
            }, assigner);
      }
      #endregion
      
    }
  }
  #region Messages
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
  public sealed partial class Any : pb::GeneratedMessage<Any, Any.Builder> {
    private Any() { }
    private static readonly Any defaultInstance = new Any().MakeReadOnly();
    private static readonly string[] _anyFieldNames = new string[] { "type_url", "value" };
    private static readonly uint[] _anyFieldTags = new uint[] { 10, 18 };
    public static Any DefaultInstance {
      get { return defaultInstance; }
    }
    
    public override Any DefaultInstanceForType {
      get { return DefaultInstance; }
    }
    
    protected override Any ThisMessage {
      get { return this; }
    }
    
    public static pbd::MessageDescriptor Descriptor {
      get { return global::google.protobuf.Proto.Any.internal__static_google_protobuf_Any__Descriptor; }
    }
    
    protected override pb::FieldAccess.FieldAccessorTable<Any, Any.Builder> InternalFieldAccessors {
      get { return global::google.protobuf.Proto.Any.internal__static_google_protobuf_Any__FieldAccessorTable; }
    }
    
    public const int TypeUrlFieldNumber = 1;
    private bool hasTypeUrl;
    private string typeUrl_ = "";
    public bool HasTypeUrl {
      get { return hasTypeUrl; }
    }
    public string TypeUrl {
      get { return typeUrl_; }
    }
    
    public const int ValueFieldNumber = 2;
    private bool hasValue;
    private pb::ByteString value_ = pb::ByteString.Empty;
    public bool HasValue {
      get { return hasValue; }
    }
    public pb::ByteString Value {
      get { return value_; }
    }
    
    public override bool IsInitialized {
      get {
        return true;
      }
    }
    
    public override void WriteTo(pb::ICodedOutputStream output) {
      CalcSerializedSize();
      string[] field_names = _anyFieldNames;
      if (hasTypeUrl) {
        output.WriteString(1, field_names[0], TypeUrl);
      }
      if (hasValue) {
        output.WriteBytes(2, field_names[1], Value);
      }
      UnknownFields.WriteTo(output);
    }
    
    private int memoizedSerializedSize = -1;
    public override int SerializedSize {
      get {
        int size = memoizedSerializedSize;
        if (size != -1) return size;
        return CalcSerializedSize();
      }
    }
    
    private int CalcSerializedSize() {
      int size = memoizedSerializedSize;
      if (size != -1) return size;
      
      size = 0;
      if (hasTypeUrl) {
        size += pb::CodedOutputStream.ComputeStringSize(1, TypeUrl);
      }
      if (hasValue) {
        size += pb::CodedOutputStream.ComputeBytesSize(2, Value);
      }
      size += UnknownFields.SerializedSize;
      memoizedSerializedSize = size;
      return size;
    }
    public static Any ParseFrom(pb::ByteString data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static Any ParseFrom(pb::ByteString data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static Any ParseFrom(byte[] data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static Any ParseFrom(byte[] data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static Any ParseFrom(global::System.IO.Stream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static Any ParseFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    public static Any ParseDelimitedFrom(global::System.IO.Stream input) {
      return CreateBuilder().MergeDelimitedFrom(input).BuildParsed();
    }
    public static Any ParseDelimitedFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return CreateBuilder().MergeDelimitedFrom(input, extensionRegistry).BuildParsed();
    }
    public static Any ParseFrom(pb::ICodedInputStream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static Any ParseFrom(pb::ICodedInputStream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    private Any MakeReadOnly() {
      return this;
    }
    
    public static Builder CreateBuilder() { return new Builder(); }
    public override Builder ToBuilder() { return CreateBuilder(this); }
    public override Builder CreateBuilderForType() { return new Builder(); }
    public static Builder CreateBuilder(Any prototype) {
      return new Builder(prototype);
    }
    
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    public sealed partial class Builder : pb::GeneratedBuilder<Any, Builder> {
      protected override Builder ThisBuilder {
        get { return this; }
      }
      public Builder() {
        result = DefaultInstance;
        resultIsReadOnly = true;
      }
      internal Builder(Any cloneFrom) {
        result = cloneFrom;
        resultIsReadOnly = true;
      }
      
      private bool resultIsReadOnly;
      private Any result;
      
      private Any PrepareBuilder() {
        if (resultIsReadOnly) {
          Any original = result;
          result = new Any();
          resultIsReadOnly = false;
          MergeFrom(original);
        }
        return result;
      }
      
      public override bool IsInitialized {
        get { return result.IsInitialized; }
      }
      
      protected override Any MessageBeingBuilt {
        get { return PrepareBuilder(); }
      }
      
      public override Builder Clear() {
        result = DefaultInstance;
        resultIsReadOnly = true;
        return this;
      }
      
      public override Builder Clone() {
        if (resultIsReadOnly) {
          return new Builder(result);
        } else {
          return new Builder().MergeFrom(result);
        }
      }
      
      public override pbd::MessageDescriptor DescriptorForType {
        get { return global::google.protobuf.Any.Descriptor; }
      }
      
      public override Any DefaultInstanceForType {
        get { return global::google.protobuf.Any.DefaultInstance; }
      }
      
      public override Any BuildPartial() {
        if (resultIsReadOnly) {
          return result;
        }
        resultIsReadOnly = true;
        return result.MakeReadOnly();
      }
      
      public override Builder MergeFrom(pb::IMessage other) {
        if (other is Any) {
          return MergeFrom((Any) other);
        } else {
          base.MergeFrom(other);
          return this;
        }
      }
      
      public override Builder MergeFrom(Any other) {
        if (other == global::google.protobuf.Any.DefaultInstance) return this;
        PrepareBuilder();
        if (other.HasTypeUrl) {
          TypeUrl = other.TypeUrl;
        }
        if (other.HasValue) {
          Value = other.Value;
        }
        this.MergeUnknownFields(other.UnknownFields);
        return this;
      }
      
      public override Builder MergeFrom(pb::ICodedInputStream input) {
        return MergeFrom(input, pb::ExtensionRegistry.Empty);
      }
      
      public override Builder MergeFrom(pb::ICodedInputStream input, pb::ExtensionRegistry extensionRegistry) {
        PrepareBuilder();
        pb::UnknownFieldSet.Builder unknownFields = null;
        uint tag;
        string field_name;
        while (input.ReadTag(out tag, out field_name)) {
          if(tag == 0 && field_name != null) {
            int field_ordinal = global::System.Array.BinarySearch(_anyFieldNames, field_name, global::System.StringComparer.Ordinal);
            if(field_ordinal >= 0)
              tag = _anyFieldTags[field_ordinal];
            else {
              if (unknownFields == null) {
                unknownFields = pb::UnknownFieldSet.CreateBuilder(this.UnknownFields);
              }
              ParseUnknownField(input, unknownFields, extensionRegistry, tag, field_name);
              continue;
            }
          }
          switch (tag) {
            case 0: {
              throw pb::InvalidProtocolBufferException.InvalidTag();
            }
            default: {
              if (pb::WireFormat.IsEndGroupTag(tag)) {
                if (unknownFields != null) {
                  this.UnknownFields = unknownFields.Build();
                }
                return this;
              }
              if (unknownFields == null) {
                unknownFields = pb::UnknownFieldSet.CreateBuilder(this.UnknownFields);
              }
              ParseUnknownField(input, unknownFields, extensionRegistry, tag, field_name);
              break;
            }
            case 10: {
              result.hasTypeUrl = input.ReadString(ref result.typeUrl_);
              break;
            }
            case 18: {
              result.hasValue = input.ReadBytes(ref result.value_);
              break;
            }
          }
        }
        
        if (unknownFields != null) {
          this.UnknownFields = unknownFields.Build();
        }
        return this;
      }
      
      
      public bool HasTypeUrl {
        get { return result.hasTypeUrl; }
      }
      public string TypeUrl {
        get { return result.TypeUrl; }
        set { SetTypeUrl(value); }
      }
      public Builder SetTypeUrl(string value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasTypeUrl = true;
        result.typeUrl_ = value;
        return this;
      }
      public Builder ClearTypeUrl() {
        PrepareBuilder();
        result.hasTypeUrl = false;
        result.typeUrl_ = "";
        return this;
      }
      
      public bool HasValue {
        get { return result.hasValue; }
      }
      public pb::ByteString Value {
        get { return result.Value; }
        set { SetValue(value); }
      }
      public Builder SetValue(pb::ByteString value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasValue = true;
        result.value_ = value;
        return this;
      }
      public Builder ClearValue() {
        PrepareBuilder();
        result.hasValue = false;
        result.value_ = pb::ByteString.Empty;
        return this;
      }
    }
    static Any() {
      object.ReferenceEquals(global::google.protobuf.Proto.Any.Descriptor, null);
    }
  }
  
  #endregion
  
}

#endregion Designer generated code