namespace Common.SecurityCrypto.SMCrypto
{
    internal class SM2SignVO
    {
        public string sm2_userd;
        public string x_coord;
        public string y_coord;
        public string sm3_z;
        public string sign_express;
        public string sm3_digest;
        public string sign_r;
        public string sign_s;
        public string verify_r;
        public string verify_s;
        public string sm2_sign;
        public string sm2_type;
        public bool isVerify;

        public string getX_coord() => x_coord;

        public void setX_coord(string x_coord) => this.x_coord = x_coord;

        public string getY_coord() => y_coord;

        public void setY_coord(string y_coord) => this.y_coord = y_coord;

        public string getSm3_z() => sm3_z;

        public void setSm3_z(string sm3_z) => this.sm3_z = sm3_z;

        public string getSm3_digest() => sm3_digest;

        public void setSm3_digest(string sm3_digest) => this.sm3_digest = sm3_digest;

        public string getSm2_signForSoft() => sm2_sign;

        public string getSm2_signForHard() => getSign_r() + getSign_s();

        public void setSm2_sign(string sm2_sign) => this.sm2_sign = sm2_sign;

        public string getSign_express() => sign_express;

        public void setSign_express(string sign_express) => this.sign_express = sign_express;

        public string getSm2_userd() => sm2_userd;

        public void setSm2_userd(string sm2_userd) => this.sm2_userd = sm2_userd;

        public string getSm2_type() => sm2_type;

        public void setSm2_type(string sm2_type) => this.sm2_type = sm2_type;

        public bool getVerify() => isVerify;

        public void setVerify(bool isVerify) => this.isVerify = isVerify;

        public string getSign_r() => sign_r;

        public void setSign_r(string sign_r) => this.sign_r = sign_r;

        public string getSign_s() => sign_s;

        public void setSign_s(string sign_s) => this.sign_s = sign_s;

        public string getVerify_r() => verify_r;

        public void setVerify_r(string verify_r) => this.verify_r = verify_r;

        public string getVerify_s() => verify_s;

        public void setVerify_s(string verify_s) => this.verify_s = verify_s;
    }
}