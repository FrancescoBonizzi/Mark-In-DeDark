export default {

    isNullOrWhitespace: (value: string | null | undefined): boolean => {
        if (value === null) {
            return true;
        }

        if (!value) {
            return true;
        }

        return value.toString().trim().length <= 0;
    },

};
