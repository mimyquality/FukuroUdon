// atlasing support
#ifndef VRCHAT_ATLASING_INCLUDED
#define VRCHAT_ATLASING_INCLUDED

#ifdef VRCHAT_ATLASING_ENABLED
    // TODO
#else
    #define VRCHAT_DEFINE_ATLAS_PROPERTY(type, name) type name
    #define VRCHAT_DEFINE_ATLAS_TEXTUREMODE(name)
    #define VRCHAT_GET_ATLAS_PROPERTY(name) name
    #define VRCHAT_ATLAS_VERTEX_INPUT
    #define VRCHAT_ATLAS_VERTEX_OUTPUT
    #define VRCHAT_TRANSFORM_ATLAS_TEX(tex, name) (tex.xy * name##_ST.xy + name##_ST.zw)
    #define VRCHAT_TRANSFORM_ATLAS_TEX_MODE(tex, name) (tex.xy * name##_ST.xy + name##_ST.zw)
    #define VRCHAT_ATLAS_INITIALIZE_VERTEX_OUTPUT(input, output)
    #define VRCHAT_ATLAS_TRANSFER_VERTEX_OUTPUT(input, output)
    #define VRCHAT_SETUP_ATLAS_INDEX_POST_VERTEX(input)
#endif
    
#endif
