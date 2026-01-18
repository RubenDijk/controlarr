import {
    LIDARR_LOAD_PATHS,
    LIDARR_LOAD_PROFILES,
    LIDARR_LOAD_METADATA_PROFILES,
    LIDARR_LOAD_TAGS,
    LIDARR_SET_CLIENT,
    LIDARR_SET_PATHS,
    LIDARR_SET_PROFILES,
    LIDARR_SET_METADATA_PROFILES,
    LIDARR_SET_TAGS
} from "../actions/LidarrClientActions";


export default function LidarrClientsReducer(state = {}, action) {
    let newState;
    let newLidarr;

    if (action.type === LIDARR_SET_CLIENT) {
        return {
            ...state,
            lidarr: action.payload.lidarr,
            client: "Lidarr"
        }
    } else if (action.type === LIDARR_LOAD_PATHS) {
        newState = { ...state };
        newLidarr = { ...newState.lidarr };
        newLidarr.isLoadingPaths = action.payload;
        newState.lidarr = newLidarr;

        return newState;
    } else if (action.type === LIDARR_SET_PATHS) {
        newState = { ...state };
        newLidarr = { ...newState.lidarr };
        newLidarr.isLoadingPaths = false;
        newLidarr.hasLoadedPaths = true;
        newLidarr.arePathsValid = action.payload.length > 0;
        newLidarr.paths = action.payload;
        newState.lidarr = newLidarr;

        return newState;
    } else if (action.type === LIDARR_LOAD_PROFILES) {
        newState = { ...state };
        newLidarr = { ...newState.lidarr };
        newLidarr.isLoadingProfiles = action.payload;
        newState.lidarr = newLidarr;

        return newState;
    } else if (action.type === LIDARR_SET_PROFILES) {
        newState = { ...state };
        newLidarr = { ...newState.lidarr };
        newLidarr.isLoadingProfiles = false;
        newLidarr.hasLoadedProfiles = true;
        newLidarr.areProfilesValid = action.payload.length > 0;
        newLidarr.profiles = action.payload;
        newState.lidarr = newLidarr;

        return newState;
    } else if (action.type === LIDARR_LOAD_METADATA_PROFILES) {
        newState = { ...state };
        newLidarr = { ...newState.lidarr };
        newLidarr.isLoadingMetadataProfiles = action.payload;
        newState.lidarr = newLidarr;

        return newState;
    } else if (action.type === LIDARR_SET_METADATA_PROFILES) {
        newState = { ...state };
        newLidarr = { ...newState.lidarr };
        newLidarr.isLoadingMetadataProfiles = false;
        newLidarr.hasLoadedMetadataProfiles = true;
        newLidarr.areMetadataProfilesValid = action.payload.length > 0;
        newLidarr.metadataProfiles = action.payload;
        newState.lidarr = newLidarr;

        return newState;
    } else if (action.type === LIDARR_LOAD_TAGS) {
        newState = { ...state };
        newLidarr = { ...newState.lidarr };
        newLidarr.isLoadingTags = action.payload;
        newState.lidarr = newLidarr;

        return newState;
    } else if (action.type === LIDARR_SET_TAGS) {
        newState = { ...state };
        newLidarr = { ...newState.lidarr };
        newLidarr.isLoadingTags = false;
        newLidarr.hasLoadedTags = true;
        newLidarr.areTagsValid = action.payload.ok;
        newLidarr.tags = action.payload.data;
        newState.lidarr = newLidarr;

        return newState;
    }

    return { ...state };
}